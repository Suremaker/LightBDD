﻿using LightBDD.Core.Formatting.Values;
using LightBDD.Core.Results.Parameters;

namespace LightBDD.Core.Execution
{
    /// <summary>
    /// Interface describing a method parameter that is verifiable.
    /// </summary>
    public interface IComplexParameter
    {
        /// <summary>
        /// Updates parameter with <see cref="IValueFormattingService"/> that should be used to format expected/actual values.
        /// </summary>
        /// <param name="formattingService">Formatting service to use.</param>
        void SetValueFormattingService(IValueFormattingService formattingService);

        /// <summary>
        /// Returns parameter details.
        /// </summary>
        IParameterDetails Details { get; }
    }
}