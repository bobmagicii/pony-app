using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp
{
    [AttributeUsage(System.AttributeTargets.Field)]
    internal class PassiveActionAttribute : Attribute
    {
    }

    [AttributeUsage(System.AttributeTargets.Field)]
    internal class ActiveActionAttribute : Attribute
    {
    }

    public static class PonyAttributeExtensions
    {
        private enum ActionType
        {
            Active,
            Passive
        }

        private static object _locker = new object();
        private static Dictionary<Tuple<PonyAction, ActionType>, bool> _dict = new Dictionary<Tuple<PonyAction, ActionType>, bool>();

        public static bool IsActive(this PonyAction action)
        {
            Tuple<PonyAction, ActionType> tuple = new Tuple<PonyAction, ActionType>(action, ActionType.Active);
            if (!_dict.ContainsKey(tuple))
            {
                lock (_locker)
                {
                    if (!_dict.ContainsKey(tuple))
                    {
                        _dict[tuple] = action
                            .GetType()
                            .GetMember(action.ToString())[0]
                            .GetCustomAttributes(typeof(ActiveActionAttribute), false)
                            .IsAny();
                    }
                }
            }

            return _dict[tuple];
        }

        public static bool IsPassive(this PonyAction action)
        {
            Tuple<PonyAction, ActionType> tuple = new Tuple<PonyAction, ActionType>(action, ActionType.Passive);
            if (!_dict.ContainsKey(tuple))
            {
                lock (_locker)
                {
                    if (!_dict.ContainsKey(tuple))
                    {
                        _dict[tuple] = action
                            .GetType()
                            .GetMember(action.ToString())[0]
                            .GetCustomAttributes(typeof(PassiveActionAttribute), false)
                            .IsAny();
                    }
                }
            }

            return _dict[tuple];
        }

        private static bool IsAny<T>(this IEnumerable<T> data)
        {
            return data != null && data.Any();
        }
    }
}
