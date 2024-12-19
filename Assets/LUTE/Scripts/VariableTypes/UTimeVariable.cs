
/*This script has been, partially or completely, generated by the GenerateVariableWindow*/
using System;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// UTime variable type.
    /// </summary>
    [VariableInfo("Date Time", "UTime")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class UTimeVariable : BaseVariable<LoGaCulture.LUTE.UTime>
    {
        public override bool SupportsArithmetic(SetOperator setOperator)
        {
            return true;
        }
        public override bool SupportsComparison()
        {
            return true;
        }

        public override bool Evaluate(ComparisonOperator comparisonOperator, UTime value)
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            TimeSpan valueDate = Value.time;

            bool condition = false;
            TimeSpan tolerance = TimeSpan.FromMilliseconds(500); // Adjust tolerance if you cannot get the condition to work

            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                    condition = Math.Abs((currentTime - valueDate).TotalMilliseconds) < tolerance.TotalMilliseconds;
                    break;
                case ComparisonOperator.NotEquals:
                    condition = Math.Abs((currentTime - valueDate).TotalMilliseconds) >= tolerance.TotalMilliseconds;
                    break;
                case ComparisonOperator.GreaterThan:
                    condition = currentTime > valueDate;
                    break;
                case ComparisonOperator.GreaterThanOrEquals:
                    condition = currentTime >= valueDate;
                    break;
                case ComparisonOperator.LessThan:
                    condition = currentTime < valueDate;
                    break;
                case ComparisonOperator.LessThanOrEquals:
                    condition = currentTime <= valueDate;
                    break;
                default:
                    condition = base.Evaluate(comparisonOperator, value);
                    break;
            }
            return condition;
        }

    }

    /// <summary>
    /// Container for a UTime variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct UTimeData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(UTimeVariable))]
        public UTimeVariable uTimeRef;

        [SerializeField]
        public LoGaCulture.LUTE.UTime uTimeVal;

        public static implicit operator LoGaCulture.LUTE.UTime(UTimeData UTimeData)
        {
            return UTimeData.Value;
        }

        public UTimeData(LoGaCulture.LUTE.UTime v)
        {
            uTimeVal = v;
            uTimeRef = null;
        }

        public LoGaCulture.LUTE.UTime Value
        {
            get { return (uTimeRef == null) ? uTimeVal : uTimeRef.Value; }
            set { if (uTimeRef == null) { uTimeVal = value; } else { uTimeRef.Value = value; } }
        }

        public string GetDescription()
        {
            if (uTimeRef == null)
            {
                return uTimeVal.ToString();
            }
            else
            {
                return uTimeRef.Key;
            }
        }
    }
}