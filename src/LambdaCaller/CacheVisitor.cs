using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LambdaCaller
{
    /// <summary>
    ///     Посетитель заданного выражения, кэширующий результаты вычисления функций.
    /// </summary>
    internal class CacheVisitor : ExpressionVisitor
    {
        // Кэш для значений функций. Пример ключа: "А(5, 5)"
        private readonly IDictionary<string, int> _cache = new Dictionary<string, int>();

        /// <summary>
        ///     Обходит заданное выражение, вычисляя его результат.
        /// </summary>
        /// <param name="node">Выражение, результат которого требуется вычислить.</param>
        /// <returns>Объект типа <see cref="ConstantExpression" />, содержащий в себе рузультат вычисления выражения.</returns>
        public override Expression Visit(Expression node)
        {
            Expression result;

            switch(node.NodeType)
            {
                case ExpressionType.UnaryPlus:
                case ExpressionType.Negate:
                    result = VisitUnary((UnaryExpression)node);
                    break;

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                    result = VisitBinary((BinaryExpression)node);
                    break;

                case ExpressionType.Call:
                    result = VisitMethodCall((MethodCallExpression)node);
                    break;

                case ExpressionType.Constant:
                    result = VisitConstant((ConstantExpression)node);
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(node.NodeType));
            }

            return result;
        }

        /// <summary>
        ///     Посещает заданное выражение с унарной операцией и вычисляет значение этой операции.
        /// </summary>
        /// <param name="unaryNode">Выражение с унарной операцией, значение которой вычисляется.</param>
        /// <returns>Выражение, содержащее константу - результат операции.</returns>
        protected override Expression VisitUnary(UnaryExpression unaryNode)
        {
            // Гарантированно будет константа,
            // так как в операнде изначально либо константа, либо вызов метода,
            // который преобразуется в константу.
            ConstantExpression constOperand = (ConstantExpression)Visit(unaryNode.Operand);
            int unaryResult = (int)constOperand.Value;

            if(unaryNode.NodeType == ExpressionType.Negate)
            {
                unaryResult *= -1;
            }

            return Expression.Constant(unaryResult);
        }

        /// <summary>
        ///     Посещает заданное выражение с бинарной операцией и вычисляет значение этой операции.
        /// </summary>
        /// <param name="node">Выражение с бинарной операцией, значение которой вычисляется.</param>
        /// <returns>Выражение, содержащее константу - результат операции.</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            // Гарантированно константы
            ConstantExpression constLeft = (ConstantExpression)Visit(node.Left);
            ConstantExpression constRight = (ConstantExpression)Visit(node.Right);

            int binaryResult = 0;

            switch(node.NodeType)
            {
                case ExpressionType.Add:
                    binaryResult = (int)constLeft.Value + (int)constRight.Value;
                    break;

                case ExpressionType.Subtract:
                    binaryResult = (int)constLeft.Value - (int)constRight.Value;
                    break;

                case ExpressionType.Multiply:
                    binaryResult = (int)constLeft.Value * (int)constRight.Value;
                    break;

                case ExpressionType.Divide:
                    binaryResult = (int)constLeft.Value / (int)constRight.Value;
                    break;
            }

            return Expression.Constant(binaryResult);
        }

        /// <summary>
        ///     Посещает заданное выражение с вызовом метода, вычисляет результат вызова, кеширует этот результат.
        /// </summary>
        /// <param name="node">Выражение с вызовом метода, результат выполнения которого вычисляется.</param>
        /// <returns>Выражение, содержащее константу - результат выполнения метода.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            IEnumerable<Expression> newArgs = VisitArguments(node.Arguments);

            node = node.Update(node.Object, newArgs);

            string cacheKey = CacheMethodCallResult(node);

            return Expression.Constant(_cache[cacheKey]);
        }

        /// <summary>
        ///     Посещает все аргументы вызова метода, вычисляя их значения.
        /// </summary>
        /// <param name="arguments">Коллекция выражений, содержащая аргументы вызова метода.</param>
        /// <returns>Коллекция, содержащая результаты вычисления значений аргументов метода.</returns>
        private IEnumerable<Expression> VisitArguments(ICollection<Expression> arguments)
        {
            Expression[] newArgs = new Expression[arguments.Count];
            int index = 0;

            foreach(Expression argument in arguments)
            {
                newArgs[index++] = Visit(argument);
            }

            return newArgs;
        }

        /// <summary>
        ///     Совершает вызов метода.
        /// </summary>
        /// <param name="methodExpression">Выражение, содержащее вызов метода.</param>
        /// <returns>Результат выполнения метода.</returns>
        private static int CallMethod(MethodCallExpression methodExpression)
        {
            MethodInfo method = methodExpression.Method;
            object obj = methodExpression.Object;

            // На данном моменте все агрументы гарантированно константы
            object[] arguments = methodExpression.Arguments.OfType<ConstantExpression>().Select(x => x.Value).ToArray();

            return (int)method.Invoke(obj, arguments);
        }

        /// <summary>
        ///     Кэширует результат выполнения метода, если его еще нет в кэше.
        /// </summary>
        /// <param name="methodExpression">Выражение, содержащее в себе вызов метода.</param>
        /// <returns>Ключ кэша, для получения результата выполнения выражения.</returns>
        private string CacheMethodCallResult(MethodCallExpression methodExpression)
        {
            string cacheKey = methodExpression.ToString();

            if(!_cache.ContainsKey(cacheKey))
            {
                _cache.Add(cacheKey, CallMethod(methodExpression));
            }

            return cacheKey;
        }
    }
}