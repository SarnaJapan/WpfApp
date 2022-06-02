﻿using KelpNet.CL.Common;
using System;

namespace KelpNet.CL
{
    public interface ICompressibleActivation<T> : CPU.ICompressibleActivation<T>, IParallelizable where T : unmanaged, IComparable<T>
    {
        ComputeKernel ForwardKernel { get; set; }
        ComputeKernel BackwardKernel { get; set; }

        //GPU向けのActivate関数の文字列
        string ActivateKernelString { get; set; } //単品で呼ぶ用

        string ForwardKernelName { get; set; }
        string BackwardKernelName { get; set; }
    }
}
