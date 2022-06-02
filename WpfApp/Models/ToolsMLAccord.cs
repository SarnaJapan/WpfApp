using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;

using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace WpfApp.Models
{
    /// <summary>
    /// 機械学習関連処理(Accord.Net)
    /// </summary>
    internal static class ToolsMLAccord
    {
        #region Parameter

        /// <summary>
        /// 教師棋譜ファイル名
        /// </summary>
        private static readonly string FILE_TRAIN = "log_ok_mcts_mcts.dat";

        /// <summary>
        /// ネットワークファイル名
        /// </summary>
        private static readonly string FILE_NN = "accord.dbn";

        /// <summary>
        /// ログファイル名
        /// </summary>
        private static readonly string FILE_LOG = "accord.log";

        /// <summary>
        /// データローダ
        /// </summary>
        private static KFDataLoader DataLoader = null;

        /// <summary>
        /// ネットワーク
        /// </summary>
        private static DeepBeliefNetwork NN = null;

        /// <summary>
        /// バッチサイズ（正規化棋譜数）
        /// </summary>
        private static readonly int BATCH_SIZE = 16;

        /// <summary>
        /// 訓練回数
        /// </summary>
        private static readonly int TRAIN_COUNT = 128;

        #endregion

        /// <summary>
        /// データローダ生成
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>データローダ</returns>
        private static KFDataLoader CreateDataLoader(string path)
        {
            if (DataLoader == null)
            {
                DataLoader = new KFDataLoader(path);
                System.Diagnostics.Debug.WriteLine("-> Load " + path);
            }
            return DataLoader;
        }

        /// <summary>
        /// ネットワーク読込
        /// </summary>
        /// <returns>ネットワーク</returns>
        private static DeepBeliefNetwork LoadNetwork()
        {
            if (NN == null)
            {
                if (File.Exists(FILE_NN))
                {
                    NN = DeepBeliefNetwork.Load(FILE_NN);
                    System.Diagnostics.Debug.WriteLine("-> Load " + GetNetworkInfo());
                }
                if (NN == null)
                {
                    // Bernoulli - Alpha
                    double BERNOULLI_ALPHA = 1.0;
                    // DBN
                    NN = new DeepBeliefNetwork(
                        function: new BernoulliFunction(BERNOULLI_ALPHA),
                        inputsCount: 128, // 黒64＋白64
                        hiddenNeurons: new int[] { 128, 96, 96, 64 } // 結果64
                    );
                    // 重み初期化
                    new GaussianWeights(NN).Randomize();
                    // 重み更新
                    NN.UpdateVisibleWeights();
                    System.Diagnostics.Debug.WriteLine("-> Create " + GetNetworkInfo());
                }
            }
            return NN;
        }

        /// <summary>
        /// 評価配列計算
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <returns>評価配列</returns>
        internal static double[] Compute(ulong p, ulong o)
        {
            LoadNetwork();
            return NN.Compute(ToolsKF.ConvInputData<double>(p, o));
        }

        /// <summary>
        /// ネットワーク訓練
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <param name="param">[string path, int type]</param>
        /// <returns>処理数</returns>
        internal static string TrainNetwork(System.IProgress<string> progress, CancellationToken token, object[] param)
        {
            // パラメータ確認
            string path = "";
            int type = 0;
            if (param.Length == 2)
            {
                path = (string)param[0];
                type = (int)param[1];
            }
            else
            {
                return "Invalid parameter.";
            }

            var res = new List<string>();
            var sw = new System.Diagnostics.Stopwatch();

            System.Diagnostics.Debug.WriteLine("Data Loading...");
            progress.Report("Data Loading...");
            CreateDataLoader(Path.Combine(path, FILE_TRAIN));

            System.Diagnostics.Debug.WriteLine("Network Loading...");
            progress.Report("Network Loading...");
            LoadNetwork();

            System.Diagnostics.Debug.WriteLine("Training Start...");
            progress.Report("Training Start...");
            res.Add($",error,,elapsed,batch={BATCH_SIZE}");

            // 教師ネットワーク
            var teacher = new DeepNeuralNetworkLearning(NN)
            {
                // Select Supervised Learning Algorithm
                Algorithm = (ann, i) => new ParallelResilientBackpropagationLearning(ann),
                // Algorithm = (ann, i) => new ResilientBackpropagationLearning(ann),
                // Algorithm = (ann, i) => new BackPropagationLearning(ann),
                // Algorithm = (ann, i) => new PerceptronLearning(ann),
                LayerIndex = NN.Layers.Length - 1,
            };

            // 1epochの処理回数
            int trainCount = DataLoader.KFList.Count / BATCH_SIZE;
            if (trainCount <= 1)
            {
                return "No/Insufficient data.";
            }

            var id = new List<double[]>();
            var ot = new List<int>();
            var od = new List<double[]>();
            for (int i = 1; i < trainCount && !token.IsCancellationRequested; i++)
            {
                id.Clear();
                ot.Clear();
                od.Clear();
                DataLoader.GetConvData8(BATCH_SIZE, id, ot);
                for (int j = 0; j < ot.Count; j++)
                {
                    od.Add(ToolsKF.ConvOutputData<double>(ot[j]));
                }

                double err = 0.0;
                double[][] oa = od.ToArray();
                double[][] layerData = teacher.GetLayerInput(id.ToArray());
                sw.Restart();
                for (int k = 0; k < TRAIN_COUNT; k++)
                {
                    err = teacher.RunEpoch(layerData, oa) / oa.Length;
                }
                sw.Stop();
                progress.Report($"C={i},E={err} @ {sw.Elapsed.TotalMilliseconds} ms");
                res.Add($",{err},,{sw.Elapsed.TotalMilliseconds},");

                NN.UpdateVisibleWeights();
            }

            System.Diagnostics.Debug.WriteLine("Network Saving...");
            progress.Report("Network Saving...");
            NN.Save(FILE_NN);
            System.Diagnostics.Debug.WriteLine("-> Save " + FILE_NN);
            Common.SaveLogList(FILE_LOG, res);

            return $"{res.Count}/{trainCount}";
        }

        /// <summary>
        /// ネットワーク強化
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <param name="param">[string path]</param>
        /// <returns>処理数</returns>
        internal static string ReinforceNetwork(System.IProgress<string> progress, CancellationToken token, object[] param)
        {
            /// @todo 実装予定
            return Common.TEST_REINFORCE;
        }

        /// <summary>
        /// ネットワーク情報取得
        /// </summary>
        /// <returns>ネットワーク情報</returns>
        private static string GetNetworkInfo()
        {
            string res = FILE_NN;
            res += " (" + NN.InputsCount + ",";
            for (int i = 0; i < NN.Layers.Length; i++)
            {
                res += NN.Layers[i].Neurons.Length + ",";
            }
            res += ")";
            return res;
        }
    }

}
