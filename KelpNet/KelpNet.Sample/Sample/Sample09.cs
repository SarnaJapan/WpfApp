﻿using System;
using System.Collections.Generic;
using KelpNet.CL;
using KelpNet.Tools;

#if DOUBLE
#elif NETSTANDARD2_1
using Math = System.MathF;
#else
using Math = KelpNet.MathF;
#endif

//using Real = System.Double;
using Real = System.Single;

namespace KelpNet.Sample
{
    //SimpleなRNNによるRNNLM
    //『Chainerによる実践深層学習』より（ISBN 978-4-274-21934-4）
    class Sample09
    {
        const int TRAINING_EPOCHS = 5;
        const int N_UNITS = 100;

        const string DOWNLOAD_URL = "https://raw.githubusercontent.com/wojzaremba/lstm/master/data/";

        const string TRAIN_FILE = "ptb.train.txt";
        const string TEST_FILE = "ptb.test.txt";

        const string TRAIN_FILE_HASH = "f26c4b92c5fdc7b3f8c7cdcb991d8420";
        const string TEST_FILE_HASH = "8b80168b89c18661a38ef683c0dc3721";

        public static void Run()
        {
            Console.WriteLine("Build Vocabulary.");

            Vocabulary vocabulary = new Vocabulary();
            string trainPath = InternetFileDownloader.Donwload(DOWNLOAD_URL + TRAIN_FILE, TRAIN_FILE, TRAIN_FILE_HASH);
            string testPath = InternetFileDownloader.Donwload(DOWNLOAD_URL + TEST_FILE, TEST_FILE, TEST_FILE_HASH);

            int[] trainData = vocabulary.LoadData(trainPath);
            int[] testData = vocabulary.LoadData(testPath);

            int nVocab = vocabulary.Length;

            Console.WriteLine("Done.");

            Console.WriteLine("Network Initilizing.");
            FunctionStack<Real> model = new FunctionStack<Real>(
                new EmbedID<Real>(nVocab, N_UNITS, name: "l1 EmbedID"),
                new Linear<Real>(N_UNITS, N_UNITS, name: "l2 Linear"),
                new TanhActivation<Real>("l2 Tanh"),
                new Linear<Real>(N_UNITS, nVocab, name: "l3 Linear"),
                new Softmax<Real>("l3 Sonftmax")
            );

            Adam<Real> adam = new Adam<Real>();
            adam.SetUp(model);

            List<int> s = new List<int>();

            Console.WriteLine("Train Start.");
            SoftmaxCrossEntropy<Real> softmaxCrossEntropy = new SoftmaxCrossEntropy<Real>();
            for (int epoch = 0; epoch < TRAINING_EPOCHS; epoch++)
            {
                for (int pos = 0; pos < trainData.Length; pos++)
                {
                    NdArray<Real> h = new NdArray<Real>(new Real[N_UNITS]);

                    int id = trainData[pos];
                    s.Add(id);

                    if (id == vocabulary.EosID)
                    {
                        Real accumloss = 0;
                        Stack<NdArray<Real>> tmp = new Stack<NdArray<Real>>();

                        for (int i = 0; i < s.Count; i++)
                        {
                            NdArray<int> tx = new NdArray<int>(i == s.Count - 1 ? new[] { vocabulary.EosID } : new[] { s[i + 1] });

                            //l1 EmbedID
                            NdArray<Real> l1 = model.Functions[0].Forward(s[i])[0];

                            //l2 Linear
                            NdArray<Real> l2 = model.Functions[1].Forward(h)[0];

                            //Add
                            NdArray<Real> xK = l1 + l2;

                            //l2 Tanh
                            h = model.Functions[2].Forward(xK)[0];

                            //l3 Linear
                            NdArray<Real> h2 = model.Functions[3].Forward(h)[0];

                            Real loss = softmaxCrossEntropy.Evaluate(h2, tx);
                            tmp.Push(h2);
                            accumloss += loss;
                        }

                        Console.WriteLine(accumloss);

                        for (int i = 0; i < s.Count; i++)
                        {
                            model.Backward(tmp.Pop());
                        }

                        adam.Update();

                        s.Clear();
                    }

                    if (pos % 100 == 0)
                    {
                        Console.WriteLine(pos + "/" + trainData.Length + " finished");
                    }
                }
            }

            Console.WriteLine("Test Start.");

            Real sum = 0;
            int wnum = 0;
            List<int> ts = new List<int>();
            bool unkWord = false;

            for (int pos = 0; pos < 1000; pos++)
            {
                int id = testData[pos];
                ts.Add(id);

                if (id > trainData.Length)
                {
                    unkWord = true;
                }

                if (id == vocabulary.EosID)
                {
                    if (!unkWord)
                    {
                        Console.WriteLine("pos" + pos);
                        Console.WriteLine("tsLen" + ts.Count);
                        Console.WriteLine("sum" + sum);
                        Console.WriteLine("wnum" + wnum);

                        sum += CalPs(model, ts);
                        wnum += ts.Count - 1;
                    }
                    else
                    {
                        unkWord = false;
                    }

                    ts.Clear();
                }
            }

            Console.WriteLine(Math.Pow(2.0f, sum / wnum));
        }

        static Real CalPs(FunctionStack<Real> model, List<int> s)
        {
            Real sum = 0;

            NdArray<Real> h = new NdArray<Real>(new Real[N_UNITS]);

            for (int i = 1; i < s.Count; i++)
            {
                //l1 Linear
                NdArray<Real> xK = model.Functions[0].Forward(s[i])[0];

                //l2 Linear
                NdArray<Real> l2 = model.Functions[1].Forward(h)[0];
                for (int j = 0; j < xK.Data.Length; j++)
                {
                    xK.Data[j] += l2.Data[j];
                }

                //l2 Tanh
                h = model.Functions[2].Forward(xK)[0];

                //l3 Softmax(l3 Linear)
                NdArray<Real> yv = model.Functions[4].Forward(model.Functions[3].Forward(h))[0];
                Real pi = yv.Data[s[i - 1]];
                sum -= Math.Log(pi, 2);
            }

            return sum;
        }
    }
}

