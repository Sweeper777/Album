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

