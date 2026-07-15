using System;
using System.Collections.Generic;

namespace Luo
{
    // 1. 空接口：给所有事件数据贴标签
    public interface IEvent { }

    // 2. 静态事件中心（全局单例）
    public class EventManager : BaseManager<EventManager>
    {
        // 核心字典：Key是事件类型(Type)，Value是委托链(Delegate)
        // 注意：这里没有用字符串，直接用 typeof(T) 做Key
        private static Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

        // 注册监听 (订阅方调用)
        public static void AddListener<T>(Action<T> listener) where T : struct, IEvent
        {
            Type eventType = typeof(T);
            if (_events.TryGetValue(eventType, out var existingDelegate))
            {
                // 合并委托，支持多个监听者
                _events[eventType] = Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                _events[eventType] = listener;
            }
        }

        // 移除监听 (订阅方在销毁时必须调用)
        public static void RemoveListener<T>(Action<T> listener) where T : struct, IEvent
        {
            Type eventType = typeof(T);
            if (_events.TryGetValue(eventType, out var existingDelegate))
            {
                var newDelegate = Delegate.Remove(existingDelegate, listener);
                if (newDelegate == null)
                    _events.Remove(eventType);
                else
                    _events[eventType] = newDelegate;
            }
        }

        // 派发事件 (发布方调用)
        public static void Dispatch<T>(T eventData) where T : struct, IEvent
        {
            Type eventType = typeof(T);
            if (_events.TryGetValue(eventType, out var existingDelegate))
            {
                // 将委托转换为具体的 Action<T> 并执行
                (existingDelegate as Action<T>)?.Invoke(eventData);
            }
        }
    }
}


