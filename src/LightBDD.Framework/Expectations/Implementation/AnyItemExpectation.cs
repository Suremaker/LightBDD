﻿using System.Collections.Generic;
using System.Linq;
using LightBDD.Core.Formatting.Values;

namespace LightBDD.Framework.Expectations.Implementation
{
    internal class AnyItemExpectation<TValue> : Expectation<IEnumerable<TValue>>
    {
        private readonly IExpectation<TValue> _itemExpectation;

        public AnyItemExpectation(IExpectation<TValue> itemExpectation)
        {
            _itemExpectation = itemExpectation;
        }

        public override ExpectationResult Verify(IEnumerable<TValue> collection, IValueFormattingService formattingService)
        {
            var errors = new List<string>();
            var i = 0;
            foreach (var item in collection ?? Enumerable.Empty<TValue>())
            {
                var result = _itemExpectation.Verify(item, formattingService);
                if (result)
                    return ExpectationResult.Success;
                errors.Add($"[{i++}]: {result.Message}");
            }

            return FormatFailure(formattingService, $"got: '{formattingService.FormatValue(collection)}'", errors);
        }

        public override string Format(IValueFormattingService formattingService)
        {
            return "any item " + _itemExpectation.Format(formattingService);
        }
    }
}