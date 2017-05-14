using System;
using System.Linq.Expressions;

namespace LambdaCaller
{
    /// <summary>
    ///     Класс для вычисления значения лямбда-выражения.
    ///     Кэширует результат выполнения методов.
    /// </summary>
    public class CacheCaller : ICaller
    {
        /// <summary>
        ///     Вычисляет значение заданного выражения.
        /// </summary>
        /// <param name="expression">Выражение, значение которого нужно вычислить.</param>
        /// <returns>Результат вычисления значения выражения.</returns>
        public int Call(Expression<Func<int>> expression)
        {
            if(expression == null)
            {
                throw new NullReferenceException();
            }

            ExpressionVisitor visitor = new CacheVisitor();

            // Гарантированно константа
            ConstantExpression result = (ConstantExpression)visitor.Visit(expression.Body);

            return (int)result.Value;
        }
    }
}