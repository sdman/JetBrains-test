using System;
using System.Linq.Expressions;
using LambdaCaller;
using Xunit;

namespace LambdaCallerTests
{
    public class CacheCallerTests
    {
        private static int _aCalledTimes;
        private static int _bCalledTimes;
        private static int _cCalledTimes;

        public CacheCallerTests()
        {
            _aCalledTimes = 0;
            _bCalledTimes = 0;
            _cCalledTimes = 0;
        }

        [Fact]
        public void Call_NullExpressionPassed_NullReferenceExceptionThrown()
        {
            // Arrange
            CacheCaller cacheCallerUnderTest = new CacheCaller();

            // Act and assert
            Assert.Throws<NullReferenceException>(() => cacheCallerUnderTest.Call(null));
        }

        [Fact]
        public void Call_ExpressionContainsVariable_ArgumentOutOfRangeThrown()
        {
            // Arrange
            int a = 5;
            Expression<Func<int>> expression = () => a;

            CacheCaller cacheCallerUnderTest = new CacheCaller();

            // Act and assert
            Assert.Throws<ArgumentOutOfRangeException>(() => cacheCallerUnderTest.Call(expression));
        }

        [Fact]
        public void Call_CalledWithoutMethods_NoMethodsCalledAndExpressionEvaluated()
        {
            // Arrange
            Expression<Func<int>> expressionWithoutMethods = () => -5 + 2 * 3 - 20 / 4;
            int expectedResult = expressionWithoutMethods.Compile().Invoke();

            CacheCaller callerUnderTest = new CacheCaller();

            // Act
            int actualResult = callerUnderTest.Call(expressionWithoutMethods);

            // Assert
            Assert.Equal(0, _aCalledTimes);
            Assert.Equal(0, _bCalledTimes);
            Assert.Equal(0, _cCalledTimes);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void Call_OneCallForEachMethod_EachMethodCalledOnce()
        {
            // Arrange
            Expression<Func<int>> expression = () => C(B(A()), 5) + 4;
            int expectedResult = expression.Compile().Invoke();
            ResetCalledTimes();

            CacheCaller caller = new CacheCaller();

            // Act
            int actualResult = caller.Call(expression);

            // Assert
            Assert.Equal(1, _aCalledTimes);
            Assert.Equal(1, _bCalledTimes);
            Assert.Equal(1, _cCalledTimes);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void Call_MethodsCalledTwoTimesWithDifferenctArgs_EachMethodExceptACalledTwice()
        {
            // Arrange
            Expression<Func<int>> expression = () => A() * A() + B(10) / B(5) - C(1, 5) + C(5, 1) + -10;
            int expectedResult = expression.Compile().Invoke();
            ResetCalledTimes();

            CacheCaller caller = new CacheCaller();

            // Act
            int actualResult = caller.Call(expression);

            // Assert
            Assert.Equal(1, _aCalledTimes);
            Assert.Equal(2, _bCalledTimes);
            Assert.Equal(2, _cCalledTimes);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void Call_MethodsCalledTwoTimesWithSameArgs_EachMethodCalledOnce()
        {
            // Arrange
            Expression<Func<int>> expression = () => A() * A() + B(5) / B(5) - C(5, 1) + -C(5, 1) + -10;
            int expectedResult = expression.Compile().Invoke();
            ResetCalledTimes();

            CacheCaller caller = new CacheCaller();

            // Act
            int actualResult = caller.Call(expression);

            // Assert
            Assert.Equal(1, _aCalledTimes);
            Assert.Equal(1, _bCalledTimes);
            Assert.Equal(1, _cCalledTimes);

            Assert.Equal(expectedResult, actualResult);
        }

        private static void ResetCalledTimes()
        {
            _aCalledTimes = 0;
            _bCalledTimes = 0;
            _cCalledTimes = 0;
        }

        private static int A()
        {
            _aCalledTimes++;
            return 0;
        }

        private static int B(int a)
        {
            _bCalledTimes++;
            return a;
        }

        private static int C(int a, int b)
        {
            _cCalledTimes++;
            return a + b;
        }
    }
}