using Album.Syntax;
using System.Linq;

namespace Album.Semantics {
    public static class OptimisationRules {
        public static readonly OptimisationRule EvaluateBinaryOperators = (ctx, newLine) => {
            if (ctx.CurrentLineCount < 2) {
                return null;
            }
            var (prevLine, secondPrevLine) = ctx.PreviousTwoLines();
            if (prevLine.IsPush(out var top) && prevLine.IsPush(out var second)) {
                int? result = null;
                switch (newLine.Type) {
                    case LineType.Add:
                        result = top + second;
                        break;
                    case LineType.And:
                        result = top & second;
                        break;
                    case LineType.Eor:
                        result = top ^ second;
                        break;
                    case LineType.Or:
                        result = top | second;
                        break;
                    case LineType.Sub:
                        result = top - second;
                        break;
                }
                if (result != null) {
                    return new OptimisationResult(
                        2, 
                        LineInfo.Push(result.Value, newLine.LineNumber).AsSingleEnumerable()
                    );
                }

            }
            return null;
        };

        public static readonly OptimisationRule EvaluateUnaryOperators = (ctx, newLine) => {
            if (ctx.CurrentLineCount < 1) {
                return null;
            }
            var prevLine = ctx.PreviousLine();
            if (prevLine.IsPush(out var top)) {
                int? result = null;
                switch (newLine.Type) {
                    case LineType.Double:
                        result = top << 1;
                        break;
                    case LineType.Halve:
                        result = top >> 1;
                        break;
                    case LineType.Dup:
                        result = top;
                        break;
                    case LineType.TopNegative:
                        result = top < 0 ? 1 : 0;
                        break;
                    case LineType.TopPositive:
                        result = top > 0 ? 1 : 0;
                        break;
                    case LineType.TopZero:
                        result = top == 0 ? 1 : 0;
                        break;
                }
                if (result != null) {
                    return new OptimisationResult(
                        1, 
                        LineInfo.Push(result.Value, newLine.LineNumber).AsSingleEnumerable()
                    );
                }
            }
            return null;
        };

