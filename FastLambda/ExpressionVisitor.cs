﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Tubumu.Utils.FastLambda
{
    /// <summary>
    /// ExpressionVisitor
    /// </summary>
    public abstract class ExpressionVisitor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected ExpressionVisitor() { }

        /// <summary>
        /// Visit
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected virtual Expression Visit(Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp);

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)exp);

                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)exp);

                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)exp);

                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);

                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);

                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);

                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);

                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp);

                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp);

                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)exp);

                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);

                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)exp);

                default:
                    throw new NotSupportedException($"Unhandled expression type: '{exp.NodeType}'");
            }
        }

        /// <summary>
        /// VisitBinding
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);

                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);

                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);

                default:
                    throw new NotSupportedException($"Unhandled binding type '{binding.BindingType}'");
            }
        }

        /// <summary>
        /// VisitElementInitializer
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        /// <summary>
        /// VisitUnary
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = Visit(u.Operand);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        /// <summary>
        /// VisitBinary
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = Visit(b.Left);
            Expression right = Visit(b.Right);
            Expression conversion = Visit(b.Conversion!);
            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                {
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                }
                else
                {
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
                }
            }
            return b;
        }

        /// <summary>
        /// VisitTypeIs
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expr = Visit(b.Expression);
            if (expr != b.Expression)
            {
                return Expression.TypeIs(expr, b.TypeOperand);
            }
            return b;
        }

        /// <summary>
        /// VisitConstant
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        /// <summary>
        /// VisitConditional
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = Visit(c.Test);
            Expression ifTrue = Visit(c.IfTrue);
            Expression ifFalse = Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        /// <summary>
        /// VisitParameter
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        /// <summary>
        /// VisitMemberAccess
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = Visit(m.Expression!);
            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }
            return m;
        }

        /// <summary>
        /// VisitMethodCall
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression obj = Visit(m.Object!);
            IEnumerable<Expression> args = VisitExpressionList(m.Arguments);
            if (obj != m.Object || args != m.Arguments)
            {
                return Expression.Call(obj, m.Method, args);
            }
            return m;
        }

        /// <summary>
        /// VisitExpressionList
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression>? list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            if (list != null)
            {
                return list.AsReadOnly();
            }
            return original;
        }

        /// <summary>
        /// VisitMemberAssignment
        /// </summary>
        /// <param name="assignment"></param>
        /// <returns></returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = Visit(assignment.Expression);
            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }

        /// <summary>
        /// VisitMemberMemberBinding
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);
            return bindings != binding.Bindings ? Expression.MemberBind(binding.Member, bindings) : binding;
        }

        /// <summary>
        /// VisitMemberListBinding
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);
            return initializers != binding.Initializers ? Expression.ListBind(binding.Member, initializers) : binding;
        }

        /// <summary>
        /// VisitBindingList
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding>? list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
            {
                return list;
            }

            return original;
        }

        /// <summary>
        /// VisitElementInitializerList
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit>? list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
            {
                return list;
            }

            return original;
        }

        /// <summary>
        /// VisitLambda
        /// </summary>
        /// <param name="lambda"></param>
        /// <returns></returns>
        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        /// <summary>
        /// VisitNew
        /// </summary>
        /// <param name="nex"></param>
        /// <returns></returns>
        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments)
            {
                if (nex.Members != null)
                {
                    return Expression.New(nex.Constructor!, args, nex.Members);
                }
                else
                {
                    return Expression.New(nex.Constructor!, args);
                }
            }
            return nex;
        }

        /// <summary>
        /// VisitMemberInit
        /// </summary>
        /// <param name="init"></param>
        /// <returns></returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }

        /// <summary>
        /// VisitListInit
        /// </summary>
        /// <param name="init"></param>
        /// <returns></returns>
        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }
            return init;
        }

        /// <summary>
        /// VisitNewArray
        /// </summary>
        /// <param name="na"></param>
        /// <returns></returns>
        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType()!, exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(na.Type.GetElementType()!, exprs);
                }
            }
            return na;
        }

        /// <summary>
        /// VisitInvocation
        /// </summary>
        /// <param name="iv"></param>
        /// <returns></returns>
        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = VisitExpressionList(iv.Arguments);
            Expression expr = Visit(iv.Expression);
            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }
            return iv;
        }
    }
}
