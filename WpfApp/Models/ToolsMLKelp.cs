using KelpNet.CL;

using System.Collections.Generic;
using System.IO;
using System.Threading;

// using Real = System.Double;
using Real = System.Single;

namespace WpfApp.Models
{
    /// <summary>
    /// 機械学習関連スタティッククラス(Kelp.Net)
    /// </summary>
    internal static class ToolsMLKelp
    {
        #region Parameter

        /// <summary>
        /// 教師棋譜ファイル名
        /// </summary>
        private static readonly string FILE_TRAIN = "log_ok_mcts_mcts.dat";

        /// <summary>
        /// ネットワークファイル名
        /// </summary>
        private static readonly string[] FILE_NN = { "kelp00.mlp", "kelp01.mlp", "kelp02.mlp", "kelp03.mlp", };

        /// <summary>
        /// ログファイル名
        /// </summary>
        private static readonly string[] FILE_LOG = { "kelp00.log", "kelp01.log", "kelp02.log", "kelp03.log", };

        /// <summary>
        /// データローダ
        /// </summary>
        private static KFDataLoader DataLoader = null;

        /// <summary>
        /// ネットワーク
        /// </summary>
        private static readonly FunctionStack<Real>[] NN = { null, null, null, null, };

        /// <summary>
        /// バッチサイズ（正規化棋譜数）
        /// </summary>
        private static readonly int BATCH_SIZE = 16;

        /// <summary>
        /// 精度確認周期
        /// </summary>
        private static readonly int EVAL_PERIOD = 100;

        #endregion

        /// <summary>
        /// データセット
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class DataSet<T> where T : unmanaged, System.IComparable<T>
        {
            /// <summary>
            /// データセット
            /// </summary>
            public KelpNet.TestDataSet<T> Data;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="loader">データローダ</param>
            /// <param name="count">要求数</param>
            public DataSet(KFDataLoader loader, int count)
            {
                int[] shape = new int[] { 2, 8, 8, };
                List<T[]> id = new List<T[]>();
                List<int> od = new List<int>();
                int res = loader.GetConvData8(count, id, od);
                T[] data = new T[KelpNet.NdArray.ShapeToLength(shape) * res];
                int[] label = new int[res];
                for (int i = 0; i < res; i++)
                {
                    T[] labeledData = id[i];
                    System.Array.Copy(labeledData, 0, data, i * labeledData.Length, labeledData.Length);
                    label[i] = od[i];
                }
                Data = new KelpNet.TestDataSet<T>(KelpNet.NdArray.Convert(data, shape, res), KelpNet.NdArray.Convert(label, new int[] { 1 }, res));
            }
        }

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
        /// <typeparam name="T"></typeparam>
        /// <param name="type">使用ネットワーク番号</param>
        /// <returns>ネットワーク</returns>
        private static FunctionStack<Real> LoadNetworkMLP(int type)
        {
            if (NN[type] == null)
            {
                if (File.Exists(FILE_NN[type]))
                {
                    NN[type] = (FunctionStack<Real>)ModelIO<Real>.Load(FILE_NN[type]);
                    System.Diagnostics.Debug.WriteLine("-> Load " + GetNetworkInfo(type));
                }
                if (NN[type] == null)
                {
                    // 中間層の数（32だと精度が低い。96,128だと収束しない。）
                    int N = 64;
                    // 8層MLP構成
                    NN[type] = new FunctionStack<Real>(
                        new Linear<Real>(2 * 8 * 8, N, name: "l1 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l1 BatchNorm"),
                        new ReLU<Real>(name: "l1 ReLU"),
                        new Linear<Real>(N, N, name: "l2 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l2 BatchNorm"),
                        new ReLU<Real>(name: "l2 ReLU"),
                        new Linear<Real>(N, N, name: "l3 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l3 BatchNorm"),
                        new ReLU<Real>(name: "l3 ReLU"),
                        new Linear<Real>(N, N, name: "l4 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l4 BatchNorm"),
                        new ReLU<Real>(name: "l4 ReLU"),
                        new Linear<Real>(N, N, name: "l5 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l5 BatchNorm"),
                        new ReLU<Real>(name: "l5 ReLU"),
                        new Linear<Real>(N, N, name: "l6 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l6 BatchNorm"),
                        new ReLU<Real>(name: "l6 ReLU"),
                        new Linear<Real>(N, N, name: "l7 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l7 BatchNorm"),
                        new ReLU<Real>(name: "l7 ReLU"),
                        new Linear<Real>(N, 64, name: "l8 Linear"),
                        new KelpNet.BatchNormalization<Real>(N, name: "l8 BatchNorm"),
                        new ReLU<Real>(name: "l8 ReLU")
                    );
                    System.Diagnostics.Debug.WriteLine("-> Create " + GetNetworkInfo(type));
                }
            }
            return NN[type];
        }

        private static FunctionStack<Real> LoadNetwork(int type)
        {
            if (NN[type] == null)
            {
                if (File.Exists(FILE_NN[type]))
                {
                    NN[type] = (FunctionStack<Real>)ModelIO<Real>.Load(FILE_NN[type]);
                    System.Diagnostics.Debug.WriteLine("-> Load " + GetNetworkInfo(type));
                }
                if (NN[type] == null)
                {
                    // 2層CNN構成
                    NN[type] = new FunctionStack<Real>(
                        new Convolution2D<Real>(2, 64, 5, pad: 2, name: "l1 Conv2D", gpuEnable: true),
                        new ReLU<Real>(name: "l1 ReLU"),
                        new MaxPooling2D<Real>(2, 2, name: "l1 MaxPooling", gpuEnable: true),
                        new Convolution2D<Real>(64, 64, 5, pad: 2, name: "l2 Conv2D", gpuEnable: true),
                        new ReLU<Real>(name: "l2 ReLU"),
                        new MaxPooling2D<Real>(2, 2, name: "l2 MaxPooling", gpuEnable: true),
                        new Linear<Real>(2 * 64, 256, name: "l3 Linear", gpuEnable: true),
                        new ReLU<Real>(name: "l3 ReLU"),
                        new Dropout<Real>(name: "l3 DropOut"),
                        new Linear<Real>(256, 64, name: "l4 Linear", gpuEnable: true)
                    );
                    System.Diagnostics.Debug.WriteLine("-> Create " + GetNetworkInfo(type));
                }
            }
            return NN[type];
        }

        /// <summary>
        /// 評価配列計算
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <param name="type">使用ネットワーク番号</param>
        /// <returns>評価配列</returns>
        internal static double[] Compute(ulong p, ulong o, int type)
        {
            var res = new double[64];
            LoadNetwork(type);
            KelpNet.NdArray<Real> id = ToolsKF.ConvInputData<Real>(p, o);
            KelpNet.NdArray<Real> od = NN[type].Predict(id)[0];
            if (res.Length == od.Length)
            {
                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = od.Data[i];
                }
            }
            return res;
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
            string path;
            int type;
            try
            {
                path = (string)param[0];
                type = (int)param[1];
            }
            catch (System.Exception)
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
            LoadNetwork(type);

            System.Diagnostics.Debug.WriteLine("Training Start...");
            progress.Report("Training Start...");
            res.Add($",loss,accuracy,elapsed,batch={BATCH_SIZE}");

            // 1epochの処理回数
            int trainCount = DataLoader.KFList.Count / BATCH_SIZE;
            if (trainCount <= 1)
            {
                return "No/Insufficient data.";
            }
            // 評価データ
            DataSet<Real> evalDataset = new DataSet<Real>(DataLoader, BATCH_SIZE);

            for (int i = 1; i < trainCount && !token.IsCancellationRequested; i++)
            {
                // 訓練データ
                DataSet<Real> dataset = new DataSet<Real>(DataLoader, BATCH_SIZE);
                sw.Restart();
                Real loss = KelpNet.Trainer.Train(NN[type], dataset.Data, new KelpNet.SoftmaxCrossEntropy<Real>(), new KelpNet.Adam<Real>());
                sw.Stop();
                if (i % EVAL_PERIOD == 0)
                {
                    // 精度確認
                    Real accuracy = KelpNet.Trainer.Accuracy(NN[type], evalDataset.Data);
                    progress.Report($"C={i},L={loss},A={accuracy} @ {sw.Elapsed.TotalMilliseconds} ms");
                    res.Add($",{loss},{accuracy},{sw.Elapsed.TotalMilliseconds},");
                }
                else
                {
                    progress.Report($"C={i},L={loss} @ {sw.Elapsed.TotalMilliseconds} ms");
                    res.Add($",{loss},,{sw.Elapsed.TotalMilliseconds},");
                }
            }

            System.Diagnostics.Debug.WriteLine("Network Saving...");
            progress.Report("Network Saving...");
            ModelIO<Real>.Save(NN[type], FILE_NN[type]);
            System.Diagnostics.Debug.WriteLine("-> Save " + FILE_NN[type]);
            if (!Common.SaveLogList(FILE_LOG[type], res))
            {
                return "Save failed.";
            }

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
            return Common.TEST_REINFORCE_KELP;
        }

        /// <summary>
        /// ネットワーク情報取得
        /// </summary>
        /// <param name="type">使用ネットワーク番号</param>
        /// <returns>ネットワーク情報</returns>
        private static string GetNetworkInfo(int type)
        {
            string res = FILE_NN[type];
            res += " (";
            for (int i = 0; i < NN[type].Functions.Length; i++)
            {
                res += NN[type].Functions[i].Name + ",";
            }
            res += ")";
            return res;
        }
    }

}
