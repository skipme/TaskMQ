using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace TaskQueue.Providers
{
    public class TaskMessage : TItemModel, IComparable
    {
        [FieldDescription(Ignore = false, Inherited = true, Required = true)]
        public string MType { get; set; }

        [FieldDescription(Ignore = false, Inherited = true, Required = false)]
        public bool Processed { get; set; }

        [FieldDescription(Ignore = false, Inherited = true, Required = false)]
        public DateTime AddedTime { get; set; }

        [FieldDescription(Ignore = false, Inherited = true, Required = false)]
        public DateTime? ProcessedTime { get; set; }

        public TaskMessage(Dictionary<string, object> holder)
            : base(holder)
        {
        }
        public TaskMessage(string mtype)
        {
            MType = mtype;
        }
        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
        public override string ItemTypeName
        {
            get
            {
                return MType;
            }
            set
            {
                MType = value;// !!!
            }
        }
        /// <summary>
        /// Get dictionary holder without inherited properties, but with MType
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetSendEnvelope()
        {
            Dictionary<string, object> di = new Dictionary<string, object>();
            Type t = this.GetType();

            ValueMap<string, RepresentedModelValue> model = RepresentedModel.FindScheme(t);

            di.Add("MType", this.MType);

            for (int i = 0; i < model.val1.Count; i++)
            {
                string k = model.val1[i];
                RepresentedModelValue v = model.val2[i];
                if (!v.Inherited)
                {
                    PropertyInfo pi = model.val2[i].propertyDescriptor;
                    object val = pi.GetValue(this, null);
                    di.Add(k, val);
                }
            }
            return di;
        }
        public override string ToString()
        {
            string result = "";
            foreach (KeyValuePair<string, object> pair in GetSendEnvelope())
            {
                result += pair.Key + " = " + pair.Value.ToString() + ", \n";
            }
            return result;
        }
        // comparator::

        public Delegate MakeComparator(TQItemSelector selector, Type delegateType)
        {
            Type MessageType = this.GetType();
            if (!typeof(TaskMessage).IsAssignableFrom(MessageType))
                throw new Exception("TaskMessage derived types only");
            ParameterExpression one = Expression.Parameter(MessageType);
            ParameterExpression other = Expression.Parameter(MessageType);
            List<Expression> body = new List<Expression>();
            MethodInfo internalCMP = typeof(IComparable).GetMethod("CompareTo");

            LabelTarget returnTarget = Expression.Label(typeof(int));
            foreach (KeyValuePair<string, TQItemSelectorParam> rule in selector.parameters)
            {
                if (rule.Value.ValueSet == TQItemSelectorSet.Equals || rule.Value.ValueSet == TQItemSelectorSet.NotEquals) { continue; }

                Expression fieldA = Expression.PropertyOrField(one, rule.Key);
                Expression fieldB = Expression.PropertyOrField(other, rule.Key);
                Expression internalComparator;
                if (rule.Value.ValueSet == TQItemSelectorSet.Ascending)
                {
                    Expression castedA = Expression.Convert(fieldA, typeof(IComparable));
                    Expression castedB = Expression.Convert(fieldB, typeof(object));
                    internalComparator = Expression.Call(castedA, internalCMP, castedB);
                }
                else
                {
                    Expression castedB = Expression.Convert(fieldA, typeof(IComparable));
                    Expression castedA = Expression.Convert(fieldB, typeof(object));
                    internalComparator = Expression.Call(castedB, internalCMP, castedA);
                }
                Expression isNEq = Expression.NotEqual(internalComparator, Expression.Constant(0));
                Expression cond = Expression.IfThen(isNEq, Expression.Return(returnTarget, internalComparator));
                body.Add(cond);
            }
            LabelExpression returnDef = Expression.Label(returnTarget, Expression.Constant(0));
            body.Add(returnDef);
            Expression expression_body = Expression.Block(body);

            return Expression.Lambda(delegateType, expression_body, one, other).Compile();
        }
        public delegate int InternalComparableDictionary(Dictionary<string, object> one, Dictionary<string, object> other);
        public static InternalComparableDictionary MakeComparatorDictionary(TQItemSelector selector)
        {
            Type KeyType = typeof(Dictionary<string, object>);

            ParameterExpression one = Expression.Parameter(KeyType);
            ParameterExpression other = Expression.Parameter(KeyType);
            List<Expression> body = new List<Expression>();
            MethodInfo internalCMP = typeof(IComparable).GetMethod("CompareTo");

            PropertyInfo indexer = KeyType.GetProperty("Item");// Dictionary<string, object>[key]

            LabelTarget returnTarget = Expression.Label(typeof(int));
            ParameterExpression varCmp = Expression.Variable(typeof(int), "cmpInt");
            foreach (KeyValuePair<string, TQItemSelectorParam> rule in selector.parameters)
            {
                if (rule.Value.ValueSet == TQItemSelectorSet.Equals || rule.Value.ValueSet == TQItemSelectorSet.NotEquals) { continue; }

                Expression fieldA = Expression.Property(one, indexer, Expression.Constant(rule.Key));
                Expression fieldB = Expression.Property(other, indexer, Expression.Constant(rule.Key));
                Expression internalComparator;
                if (rule.Value.ValueSet == TQItemSelectorSet.Ascending)
                {
                    Expression castedA = Expression.Convert(fieldA, typeof(IComparable));
                    Expression castedB = Expression.Convert(fieldB, typeof(object));
                    internalComparator = Expression.Call(castedA, internalCMP, castedB);
                }
                else
                {
                    Expression castedA = Expression.Convert(fieldB, typeof(IComparable));
                    Expression castedB = Expression.Convert(fieldA, typeof(object));
                    internalComparator = Expression.Call(castedA, internalCMP, castedB);
                }
                Expression setexp = Expression.Assign(varCmp, internalComparator);
                body.Add(setexp);
                Expression isNEq = Expression.NotEqual(varCmp, Expression.Constant(0));
                Expression cond = Expression.IfThen(isNEq, Expression.Return(returnTarget, varCmp));
                body.Add(cond);
            }
            LabelExpression returnDef = Expression.Label(returnTarget, Expression.Constant(0));
            body.Add(returnDef);
            Expression expression_body = Expression.Block(new[] { varCmp }, body);

            return (InternalComparableDictionary)Expression.Lambda(typeof(InternalComparableDictionary), expression_body, one, other).Compile();
        }
        public delegate bool InternalCheckDictionary(Dictionary<string, object> one);

        /// <summary>
        /// This will check for all required keys in dictionary and
        /// CHECK Equal or NotEqual valued parameters by selector
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static InternalCheckDictionary MakeCheckerDictionary(TQItemSelector selector)
        {
            Type KeyType = typeof(Dictionary<string, object>);

            ParameterExpression one = Expression.Parameter(KeyType);

            List<Expression> body = new List<Expression>();

            MethodInfo tryGetValue = KeyType.GetMethod("TryGetValue", new Type[] { typeof(string), typeof(object).MakeByRefType() });// string, out object
            MethodInfo conv = typeof(RepresentedModel).GetMethod("Convert");

            LabelTarget returnTarget = Expression.Label(typeof(bool));
            ParameterExpression varOut = Expression.Variable(typeof(object), "outObject");

            foreach (KeyValuePair<string, TQItemSelectorParam> rule in selector.parameters)
            {
                Expression callOut = Expression.Call(one, tryGetValue, Expression.Constant(rule.Key), varOut);

                Expression cond = Expression.IfThen(Expression.Not(callOut), Expression.Return(returnTarget, Expression.Constant(false)));
                body.Add(cond);
                Expression isEq = null;
                if (rule.Value.ValueSet == TQItemSelectorSet.Equals)
                {
                    Expression cmpVal = Expression.Constant(rule.Value.Value);
                    MethodInfo internalCMP = rule.Value.Value.GetType().GetMethod("CompareTo", new Type[] { typeof(object) });
                    Expression ICMP = Expression.Call(cmpVal, internalCMP, Expression.Call(null, conv, varOut, Expression.Constant(rule.Value.Value.GetType())));
                    isEq = Expression.NotEqual(ICMP, Expression.Constant(0));
                    Expression cond2 = Expression.IfThen(isEq, Expression.Return(returnTarget, Expression.Constant(false)));
                    body.Add(cond2);
                }
                else if (rule.Value.ValueSet == TQItemSelectorSet.NotEquals)
                {
                    Expression cmpVal = Expression.Constant(rule.Value.Value);
                    MethodInfo internalCMP = rule.Value.Value.GetType().GetMethod("CompareTo", new Type[] { typeof(object) });
                    Expression ICMP = Expression.Call(cmpVal, internalCMP, Expression.Call(null, conv, varOut, Expression.Constant(rule.Value.Value.GetType())));
                    isEq = Expression.Equal(ICMP, Expression.Constant(0));
                    Expression cond2 = Expression.IfThen(isEq, Expression.Return(returnTarget, Expression.Constant(false)));
                    body.Add(cond2);
                }
            }
            LabelExpression returnDef = Expression.Label(returnTarget, Expression.Constant(true));
            body.Add(returnDef);
            Expression expression_body = Expression.Block(new[] { varOut }, body);

            return (InternalCheckDictionary)Expression.Lambda(typeof(InternalCheckDictionary), expression_body, one).Compile();
        }
        public static bool CheckWithSelector(Dictionary<string, object> other, TQItemSelector selector)
        {
            object DVAL;
            foreach (KeyValuePair<string, TQItemSelectorParam> rule in selector.parameters)
            {
                if (other.TryGetValue(rule.Key, out DVAL))
                {
                    if (rule.Value.ValueSet == TQItemSelectorSet.Equals)
                    {
                        if (((IComparable)rule.Value.Value)
                            .CompareTo(RepresentedModel.Convert(DVAL, rule.Value.Value.GetType())) != 0)
                            return false;
                    }
                    else if (rule.Value.ValueSet == TQItemSelectorSet.NotEquals)
                    {
                        if (((IComparable)rule.Value.Value)
                               .CompareTo(RepresentedModel.Convert(DVAL, rule.Value.Value.GetType())) == 0)
                            return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public static int CompareWithSelector(Dictionary<string, object> one, Dictionary<string, object> other, TQItemSelector selector)
        {
            foreach (KeyValuePair<string, TQItemSelectorParam> rule in selector.parameters)
            {
                object obja = one[rule.Key];
                object objb = other[rule.Key];
                int rslt;
                if (rule.Value.ValueSet == TQItemSelectorSet.Ascending)
                {
                    rslt = ((IComparable)obja).CompareTo(objb);
                }
                else
                {
                    rslt = ((IComparable)objb).CompareTo(obja);
                }
                if (rslt != 0)
                    return rslt;
            }
            return 0;
        }

        public int CompareTo(object obj)
        {
            return object.ReferenceEquals(this, obj) ? 0 : 1;
        }
    }
    /// <summary>
    /// Message Dictionary Comparer/Checker
    /// Maker sure Holder updated before use
    /// </summary>
    public class MessageComparer : IComparer<TaskMessage>
    {
        TaskMessage.InternalComparableDictionary Comparator;
        TaskMessage.InternalCheckDictionary Checker;
        public MessageComparer(TQItemSelector selector)
        {
            Comparator = TaskMessage.MakeComparatorDictionary(selector);
            Checker = TaskMessage.MakeCheckerDictionary(selector);
        }
        public bool Check(TaskMessage msg)
        {
            return Checker(msg.Holder);
        }
        public int Compare(TaskMessage x, TaskMessage y)
        {
            return Comparator(x.Holder, y.Holder);
        }
    }
}
