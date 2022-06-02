﻿using System;
#if DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace KelpNet
{
#if !DOUBLE
    public class MomentumSGD<T> : MomentumOptimizer<T> where T : unmanaged, IComparable<T>
    {
        public T Momentum;

        public MomentumSGD(T? learningRate = null, T? momentum = null)
        {
            this.LearningRate = learningRate ?? (TVal<T>)0.01;
            this.Momentum = momentum ?? (TVal<T>)0.9;

            switch (this)
            {
                case MomentumSGD<float> momentumSgdF:
                    momentumSgdF.Update = () => OptimizerF.Update(momentumSgdF);
                    momentumSgdF.UpdateFunctionParameters = (i) => MomentumSGDF.UpdateFunctionParameters(momentumSgdF.LearningRate, momentumSgdF.Momentum, momentumSgdF.var[i], momentumSgdF.FunctionParameters[i]);
                    break;

                case MomentumSGD<double> momentumSgdD:
                    momentumSgdD.Update = () => OptimizerD.Update(momentumSgdD);
                    momentumSgdD.UpdateFunctionParameters = (i) => MomentumSGDD.UpdateFunctionParameters(momentumSgdD.LearningRate, momentumSgdD.Momentum, momentumSgdD.var[i], momentumSgdD.FunctionParameters[i]);
                    break;
            }
        }

        protected override void AddFunctionParameters(NdArray<T>[] functionParameters)
        {
            foreach (NdArray<T> functionParameter in functionParameters)
            {
                this.var.Add(new T[functionParameter.Data.Length]);
            }
        }
    }
#endif

#if DOUBLE
    public static class MomentumSGDD
#else
    public static class MomentumSGDF
#endif
    {
        public static void UpdateFunctionParameters(Real learningRate, Real momentum, Real[] v, NdArray<Real> functionParameter)
        {
            for (int i = 0; i < functionParameter.Data.Length; i++)
            {
                v[i] *= momentum;
                v[i] -= learningRate * functionParameter.Grad[i];

                functionParameter.Data[i] += v[i];
            }
        }
    }
}
