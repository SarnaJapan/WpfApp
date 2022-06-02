﻿using KelpNet.CL;
using KelpNet.Tools;
using System;
using System.Diagnostics;
//using Real = System.Double;
using Real = System.Single;

namespace KelpNet.Sample
{
    class Sample18
    {
        //ミニバッチの数
        const int BATCH_DATA_COUNT = 64;

        //一世代あたりの訓練回数
        const int TRAIN_DATA_COUNT = 3000; // = 60000 / 20

        //性能評価時のデータ数
        const int TEACH_DATA_COUNT = 200;

        public static void Run(bool isCifar100 = false, bool isFineLabel = false)
        {
            Stopwatch sw = new Stopwatch();

            //CIFARのデータを用意する
            Console.WriteLine("CIFAR Data Loading...");
            CifarData<Real> cifarData = new CifarData<Real>(isCifar100, isFineLabel);

            //ネットワークの構成を FunctionStack に書き連ねる
            FunctionStack<Real> nn = new FunctionStack<Real>(
                new Convolution2D<Real>(3, 32, 3, name: "l1 Conv2D", gpuEnable: true),
                new BatchNormalization<Real>(32, name: "l1 BatchNorm"),
                new ReLU<Real>(name: "l1 ReLU"),
                new MaxPooling2D<Real>(2, name: "l1 MaxPooling", gpuEnable: true),
                new Convolution2D<Real>(32, 64, 3, name: "l2 Conv2D", gpuEnable: true),
                new BatchNormalization<Real>(64, name: "l1 BatchNorm"),
                new ReLU<Real>(name: "l2 ReLU"),
                new MaxPooling2D<Real>(2, 2, name: "l2 MaxPooling", gpuEnable: true),
                new Linear<Real>(14 * 14 * 64, 512, name: "l3 Linear", gpuEnable: true),
                new ReLU<Real>(name: "l3 ReLU"),
                //Cifar100のときは100クラス、簡素であれば20クラス、Cifar10のときは10クラス分類
                new Linear<Real>(512, cifarData.ClassCount, name: "l4 Linear", gpuEnable: true)
            );

            Console.WriteLine("Training Start...");

            //三世代学習
            for (int epoch = 1; epoch < 3; epoch++)
            {
                Console.WriteLine("epoch " + epoch);

                //全体での誤差を集計
                Real totalLoss = 0;
                long totalLossCount = 0;

                //何回バッチを実行するか
                for (int i = 1; i < TRAIN_DATA_COUNT + 1; i++)
                {
                    sw.Restart();

                    Console.WriteLine("\nbatch count " + i + "/" + TRAIN_DATA_COUNT);

                    //訓練データからランダムにデータを取得
                    TestDataSet<Real> datasetX = cifarData.Train.GetRandomDataSet(BATCH_DATA_COUNT);

                    //バッチ学習を並列実行する
                    Real sumLoss = Trainer.Train(nn, datasetX, new SoftmaxCrossEntropy<Real>(), new Adam<Real>());
                    totalLoss += sumLoss;
                    totalLossCount++;

                    //結果出力
                    Console.WriteLine("total loss " + totalLoss / totalLossCount);
                    Console.WriteLine("local loss " + sumLoss);

                    sw.Stop();
                    Console.WriteLine("time" + sw.Elapsed.TotalMilliseconds);

                    //20回バッチを動かしたら精度をテストする
                    if (i % 20 == 0)
                    {
                        Console.WriteLine("\nTesting...");

                        //テストデータからランダムにデータを取得
                        TestDataSet<Real> datasetY = cifarData.Eval.GetRandomDataSet(TEACH_DATA_COUNT);

                        //テストを実行
                        Real accuracy = Trainer.Accuracy(nn, datasetY);
                        Console.WriteLine("accuracy " + accuracy);
                    }
                }
            }
        }
    }
}
