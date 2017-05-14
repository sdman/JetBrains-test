using System;
using System.Linq.Expressions;

namespace LambdaCaller
{
    /// <summary>
    ///     Интерфейс для вычисления значения лямбда-выражения.
    ///     Был дан в задании.
    /// </summary>
    public interface ICaller
    {
        /// <summary>
        ///     Вычисляет значение заданного выражения.
        /// </summary>
        /// <param name="expression">Выражение, значение которого нужно вычислить.</param>
        /// <returns>Результат вычисления значения выражения.</returns>
        int Call(Expression<Func<int>> expression);
    }
}