using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace NoMoreLeaks
{
    internal static class EventCleanup
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Assembly StockAssembly = Assembly.GetAssembly(typeof(Part));

        internal static void RemoveGameEvent(object eventSource, object owner, string methodName)
        {
            RemoveGameEvent(eventSource, owner, null, methodName);
        }

        internal static void RemoveGameEvent(object eventSource, object owner, Type handlerDeclaringType, string methodName)
        {
            if (eventSource == null || owner == null) return;

            int beforeCount = GetEventEntryCount(eventSource);

            MethodInfo handlerMethod = handlerDeclaringType != null
                ? handlerDeclaringType.GetMethod(methodName, AnyInstance)
                : FindInstanceMethod(owner.GetType(), methodName);

            if (handlerMethod == null)
            {
                string typeName = handlerDeclaringType != null ? handlerDeclaringType.FullName : owner.GetType().FullName;
                Debug.LogWarning("[NoMoreLeaks] Missing handler " + typeName + "." + methodName);
                return;
            }

            MethodInfo removeMethod = FindSingleDelegateParameterMethod(eventSource.GetType(), "Remove");
            if (removeMethod == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing Remove(delegate) on " + eventSource.GetType().FullName);
                return;
            }

            Type delegateType = removeMethod.GetParameters()[0].ParameterType;
            Delegate handler = Delegate.CreateDelegate(delegateType, owner, handlerMethod, false);
            if (handler == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Could not bind " + owner.GetType().FullName + "." + methodName + " as " + delegateType.FullName);
                return;
            }

            removeMethod.Invoke(eventSource, new object[] { handler });

            int afterCount = GetEventEntryCount(eventSource);
            int removed = beforeCount >= 0 && afterCount >= 0 ? beforeCount - afterCount : 0;
            if (removed > 0)
                LogRemoval("RemoveGameEvent", DescribeEventSource(eventSource), owner.GetType(), removed);
        }

        internal static void RemoveInstanceEventField(object source, string fieldName, object owner, string methodName)
        {
            if (source == null) return;

            FieldInfo field = AccessTools.Field(source.GetType(), fieldName);
            if (field == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing field " + source.GetType().FullName + "." + fieldName);
                return;
            }

            RemoveGameEvent(field.GetValue(source), owner, methodName);
        }

        internal static void RemoveStaticDelegateField(Type type, string fieldName, object owner, string methodName)
        {
            if (type == null || owner == null) return;

            FieldInfo field = type.GetField(fieldName, AnyStatic);
            if (field == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing static delegate field " + type.FullName + "." + fieldName);
                return;
            }

            Delegate current = field.GetValue(null) as Delegate;
            if (current == null) return;

            MethodInfo handlerMethod = FindInstanceMethod(owner.GetType(), methodName);
            if (handlerMethod == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing handler " + owner.GetType().FullName + "." + methodName);
                return;
            }

            Delegate handler = Delegate.CreateDelegate(field.FieldType, owner, handlerMethod, false);
            if (handler == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Could not bind " + owner.GetType().FullName + "." + methodName + " as " + field.FieldType.FullName);
                return;
            }

            int beforeCount = current.GetInvocationList().Length;
            field.SetValue(null, Delegate.Remove(current, handler));
            Delegate updated = field.GetValue(null) as Delegate;
            int afterCount = updated != null ? updated.GetInvocationList().Length : 0;
            int removed = beforeCount - afterCount;
            if (removed > 0)
                LogRemoval("RemoveStaticDelegateField", type.FullName + "." + fieldName, owner.GetType(), removed);
        }

        internal static int RemoveDestroyedStaticDelegateOwners(Type type, string fieldName, Type ownerType)
        {
            if (type == null || string.IsNullOrEmpty(fieldName) || ownerType == null) return 0;

            FieldInfo field = type.GetField(fieldName, AnyStatic);
            if (field == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing static delegate field " + type.FullName + "." + fieldName);
                return 0;
            }

            Delegate current = field.GetValue(null) as Delegate;
            if (current == null) return 0;

            Delegate cleaned = current;
            int removed = 0;
            foreach (Delegate callback in current.GetInvocationList())
            {
                object target = callback.Target;
                Type targetType = target != null ? target.GetType() : null;
                if (!OwnerMatches(ownerType, target, targetType != null ? targetType.FullName : null)) continue;

                UnityEngine.Object unityObject = target as UnityEngine.Object;
                bool isDestroyedUnityObject = !ReferenceEquals(unityObject, null) && unityObject == null;
                if (target != null && !isDestroyedUnityObject) continue;

                cleaned = Delegate.Remove(cleaned, callback);
                removed++;
            }

            if (removed == 0) return 0;

            field.SetValue(null, cleaned);
            LogRemoval("RemoveDestroyedStaticDelegateOwners", type.FullName + "." + fieldName, ownerType, removed);
            return removed;
        }

        internal static void RemoveDelegatesOwnedBy(object eventOwner, string delegatePropertyName, object callbackOwner)
        {
            if (eventOwner == null || callbackOwner == null) return;

            Type type = eventOwner.GetType();
            PropertyInfo property = type.GetProperty(delegatePropertyName, AnyInstance);
            FieldInfo field = type.GetField(delegatePropertyName, AnyInstance);

            Delegate current = property != null
                ? property.GetValue(eventOwner, null) as Delegate
                : field != null ? field.GetValue(eventOwner) as Delegate : null;

            if (current == null) return;

            Delegate cleaned = current;
            int removed = 0;
            foreach (Delegate callback in current.GetInvocationList())
            {
                if (ReferenceEquals(callback.Target, callbackOwner))
                {
                    cleaned = Delegate.Remove(cleaned, callback);
                    removed++;
                }
            }

            if (property != null)
                property.SetValue(eventOwner, cleaned, null);
            else if (field != null)
                field.SetValue(eventOwner, cleaned);
            else
                Debug.LogWarning("[NoMoreLeaks] Missing delegate member " + type.FullName + "." + delegatePropertyName);

            if (removed > 0)
                LogRemoval("RemoveDelegatesOwnedBy", type.FullName + "." + delegatePropertyName, callbackOwner.GetType(), removed);
        }

        internal static int RemoveDestroyedOwners(object eventSource, Type ownerType)
        {
            if (eventSource == null || ownerType == null) return 0;

            FieldInfo eventsField = AccessTools.Field(eventSource.GetType(), "events");
            if (eventsField == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing events list on " + eventSource.GetType().FullName);
                return 0;
            }

            IList events = eventsField.GetValue(eventSource) as IList;
            if (events == null) return 0;

            int removed = 0;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                object eventEntry = events[i];
                if (eventEntry == null) continue;

                FieldInfo originatorField = AccessTools.Field(eventEntry.GetType(), "originator");
                if (originatorField == null)
                {
                    Debug.LogWarning("[NoMoreLeaks] Missing event originator field on " + eventEntry.GetType().FullName);
                    return removed;
                }

                object originator = originatorField.GetValue(eventEntry);
                string originatorTypeName = GetOriginatorTypeName(eventEntry);
                if (!OwnerMatches(ownerType, originator, originatorTypeName)) continue;

                UnityEngine.Object unityObject = originator as UnityEngine.Object;
                bool isDestroyedUnityObject = !ReferenceEquals(unityObject, null) && unityObject == null;
                if (originator != null && !isDestroyedUnityObject) continue;

                events.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
                LogRemoval("RemoveDestroyedOwners", DescribeEventSource(eventSource), ownerType, removed);

            return removed;
        }

        internal static int RemoveDestroyedOwnersByTypeName(object eventSource, string ownerTypeName)
        {
            if (eventSource == null || string.IsNullOrEmpty(ownerTypeName)) return 0;

            FieldInfo eventsField = AccessTools.Field(eventSource.GetType(), "events");
            if (eventsField == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing events list on " + eventSource.GetType().FullName);
                return 0;
            }

            IList events = eventsField.GetValue(eventSource) as IList;
            if (events == null) return 0;

            int removed = 0;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                object eventEntry = events[i];
                if (eventEntry == null) continue;

                FieldInfo originatorField = AccessTools.Field(eventEntry.GetType(), "originator");
                if (originatorField == null)
                {
                    Debug.LogWarning("[NoMoreLeaks] Missing event originator field on " + eventEntry.GetType().FullName);
                    return removed;
                }

                object originator = originatorField.GetValue(eventEntry);
                string originatorTypeName = GetOriginatorTypeName(eventEntry);

                Type originatorType = originator != null ? originator.GetType() : null;
                bool typeMatches = TypeNameMatches(originatorType, originatorTypeName, ownerTypeName);
                if (!typeMatches) continue;

                UnityEngine.Object unityObject = originator as UnityEngine.Object;
                bool isDestroyedUnityObject = !ReferenceEquals(unityObject, null) && unityObject == null;
                if (originator != null && !isDestroyedUnityObject) continue;

                events.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
                LogRemoval("RemoveDestroyedOwnersByTypeName", DescribeEventSource(eventSource), ownerTypeName, removed);

            return removed;
        }

        internal static int RemoveOwner(object eventSource, object owner)
        {
            if (eventSource == null || owner == null) return 0;

            FieldInfo eventsField = AccessTools.Field(eventSource.GetType(), "events");
            if (eventsField == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing events list on " + eventSource.GetType().FullName);
                return 0;
            }

            IList events = eventsField.GetValue(eventSource) as IList;
            if (events == null) return 0;

            int removed = 0;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                object eventEntry = events[i];
                if (eventEntry == null) continue;

                FieldInfo originatorField = AccessTools.Field(eventEntry.GetType(), "originator");
                if (originatorField == null)
                {
                    Debug.LogWarning("[NoMoreLeaks] Missing event originator field on " + eventEntry.GetType().FullName);
                    return removed;
                }

                if (!ReferenceEquals(originatorField.GetValue(eventEntry), owner)) continue;

                events.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
                LogRemoval("RemoveOwner", DescribeEventSource(eventSource), owner.GetType(), removed);

            return removed;
        }

        internal static int RemoveOwnersWhere(object eventSource, Func<object, bool> shouldRemove, string ownerDescription)
        {
            if (eventSource == null || shouldRemove == null || string.IsNullOrEmpty(ownerDescription)) return 0;

            FieldInfo eventsField = AccessTools.Field(eventSource.GetType(), "events");
            if (eventsField == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing events list on " + eventSource.GetType().FullName);
                return 0;
            }

            IList events = eventsField.GetValue(eventSource) as IList;
            if (events == null) return 0;

            int removed = 0;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                object eventEntry = events[i];
                if (eventEntry == null) continue;

                FieldInfo originatorField = AccessTools.Field(eventEntry.GetType(), "originator");
                if (originatorField == null)
                {
                    Debug.LogWarning("[NoMoreLeaks] Missing event originator field on " + eventEntry.GetType().FullName);
                    return removed;
                }

                object originator = originatorField.GetValue(eventEntry);
                if (!shouldRemove(originator)) continue;

                events.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
                LogRemoval("RemoveOwnersWhere", DescribeEventSource(eventSource), ownerDescription, removed);

            return removed;
        }

        internal static int RemoveDestroyedStockGameEventOwners()
        {
            IDictionary eventsByName = GetStaticMember(typeof(BaseGameEvent), "eventsByName") as IDictionary;
            if (eventsByName == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Missing BaseGameEvent.eventsByName");
                return 0;
            }

            ArrayList eventSources = new ArrayList();
            try
            {
                foreach (DictionaryEntry eventEntry in eventsByName)
                    eventSources.Add(eventEntry.Value);
            }
            catch (InvalidOperationException)
            {
                NoMoreLeaksSettings.LogDebug("Deferred broad stock sweep because the GameEvents registry changed");
                return 0;
            }

            int removed = 0;
            for (int i = 0; i < eventSources.Count; i++)
                removed += RemoveDestroyedAssemblyOwnersFromEventSource(eventSources[i], StockAssembly);

            return removed;
        }

        internal static int RemoveDestroyedDelegateMemberOwners(object source, string delegateMemberName)
        {
            if (source == null || string.IsNullOrEmpty(delegateMemberName)) return 0;

            Type type = source.GetType();
            PropertyInfo property = type.GetProperty(delegateMemberName, AnyInstance);
            FieldInfo field = type.GetField(delegateMemberName, AnyInstance);
            Delegate current = property != null
                ? property.GetValue(source, null) as Delegate
                : field != null ? field.GetValue(source) as Delegate : null;

            if (current == null) return 0;

            Delegate cleaned = current;
            int removed = 0;
            Dictionary<string, int> removedByType = NoMoreLeaksSettings.VerboseDebugLogging
                ? new Dictionary<string, int>()
                : null;
            foreach (Delegate callback in current.GetInvocationList())
            {
                if (!IsDestroyedAssemblyOwnedUnityObject(callback.Target, StockAssembly)) continue;

                cleaned = Delegate.Remove(cleaned, callback);
                removed++;

                if (removedByType != null && callback.Target != null)
                {
                    string ownerTypeName = FormatTypeName(callback.Target.GetType());
                    int currentCount;
                    removedByType.TryGetValue(ownerTypeName, out currentCount);
                    removedByType[ownerTypeName] = currentCount + 1;
                }
            }

            if (removed == 0) return 0;

            if (property != null)
                property.SetValue(source, cleaned, null);
            else if (field != null)
                field.SetValue(source, cleaned);

            if (removedByType != null)
            {
                string eventName = type.FullName + "." + delegateMemberName;
                foreach (KeyValuePair<string, int> entry in removedByType)
                    LogRemoval("RemoveDestroyedDelegateMemberOwners", eventName, entry.Key, entry.Value);
            }

            return removed;
        }

        internal static int RemoveDestroyedDelegateMembers(object source, string sourceName)
        {
            if (source == null) return 0;

            Type type = source.GetType();
            int removed = 0;

            PropertyInfo[] properties = type.GetProperties(AnyInstance);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (!property.CanRead || !property.CanWrite) continue;
                if (property.GetIndexParameters().Length != 0) continue;
                if (!IsDelegateType(property.PropertyType)) continue;

                Delegate current = property.GetValue(source, null) as Delegate;
                if (current == null) continue;

                Delegate cleaned = current;
                int localRemoved = 0;
                Dictionary<string, int> removedByType = NoMoreLeaksSettings.VerboseDebugLogging
                    ? new Dictionary<string, int>()
                    : null;

                foreach (Delegate callback in current.GetInvocationList())
                {
                    if (!IsDestroyedAssemblyOwnedUnityObject(callback.Target, StockAssembly)) continue;

                    cleaned = Delegate.Remove(cleaned, callback);
                    localRemoved++;

                    if (removedByType != null && callback.Target != null)
                    {
                        string ownerTypeName = FormatTypeName(callback.Target.GetType());
                        int currentCount;
                        removedByType.TryGetValue(ownerTypeName, out currentCount);
                        removedByType[ownerTypeName] = currentCount + 1;
                    }
                }

                if (localRemoved == 0) continue;

                property.SetValue(source, cleaned, null);
                removed += localRemoved;

                if (removedByType != null)
                {
                    string eventName = sourceName + "." + property.Name;
                    foreach (KeyValuePair<string, int> entry in removedByType)
                        LogRemoval("RemoveDestroyedDelegateMembers", eventName, entry.Key, entry.Value);
                }
            }

            FieldInfo[] fields = type.GetFields(AnyInstance);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!IsDelegateType(field.FieldType)) continue;

                Delegate current = field.GetValue(source) as Delegate;
                if (current == null) continue;

                Delegate cleaned = current;
                int localRemoved = 0;
                Dictionary<string, int> removedByType = NoMoreLeaksSettings.VerboseDebugLogging
                    ? new Dictionary<string, int>()
                    : null;

                foreach (Delegate callback in current.GetInvocationList())
                {
                    if (!IsDestroyedAssemblyOwnedUnityObject(callback.Target, StockAssembly)) continue;

                    cleaned = Delegate.Remove(cleaned, callback);
                    localRemoved++;

                    if (removedByType != null && callback.Target != null)
                    {
                        string ownerTypeName = FormatTypeName(callback.Target.GetType());
                        int currentCount;
                        removedByType.TryGetValue(ownerTypeName, out currentCount);
                        removedByType[ownerTypeName] = currentCount + 1;
                    }
                }

                if (localRemoved == 0) continue;

                field.SetValue(source, cleaned);
                removed += localRemoved;

                if (removedByType != null)
                {
                    string eventName = sourceName + "." + field.Name;
                    foreach (KeyValuePair<string, int> entry in removedByType)
                        LogRemoval("RemoveDestroyedDelegateMembers", eventName, entry.Key, entry.Value);
                }
            }

            return removed;
        }

        private static int RemoveDestroyedAssemblyOwnersFromEventSource(object eventSource, Assembly assembly)
        {
            if (eventSource == null || assembly == null) return 0;

            FieldInfo eventsField = AccessTools.Field(eventSource.GetType(), "events");
            if (eventsField == null) return 0;

            IList events = eventsField.GetValue(eventSource) as IList;
            if (events == null) return 0;

            int removed = 0;
            Dictionary<string, int> removedByType = NoMoreLeaksSettings.VerboseDebugLogging
                ? new Dictionary<string, int>()
                : null;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                object eventEntry = events[i];
                if (eventEntry == null) continue;

                FieldInfo originatorField = AccessTools.Field(eventEntry.GetType(), "originator");
                if (originatorField == null) continue;

                object originator = originatorField.GetValue(eventEntry);
                if (!IsDestroyedAssemblyOwnedUnityObject(originator, assembly)) continue;

                events.RemoveAt(i);
                removed++;

                if (removedByType != null)
                {
                    string ownerTypeName = FormatTypeName(originator.GetType());
                    int currentCount;
                    removedByType.TryGetValue(ownerTypeName, out currentCount);
                    removedByType[ownerTypeName] = currentCount + 1;
                }
            }

            if (removedByType != null)
            {
                string eventName = DescribeEventSource(eventSource);
                foreach (KeyValuePair<string, int> entry in removedByType)
                    LogRemoval("RemoveDestroyedStockGameEventOwners", eventName, entry.Key, entry.Value);
            }

            return removed;
        }

        private static bool IsDestroyedAssemblyOwnedUnityObject(object owner, Assembly assembly)
        {
            if (owner == null || assembly == null) return false;

            UnityEngine.Object unityObject = owner as UnityEngine.Object;
            if (ReferenceEquals(unityObject, null)) return false;
            if (unityObject != null) return false;

            return owner.GetType().Assembly == assembly;
        }

        private static bool IsDelegateType(Type type)
        {
            return type != null && typeof(Delegate).IsAssignableFrom(type);
        }

        private static bool OwnerMatches(Type ownerType, object originator, string originatorTypeName)
        {
            if (originator != null && ownerType.IsInstanceOfType(originator)) return true;
            return TypeNameMatches(null, originatorTypeName, ownerType.Name)
                || TypeNameMatches(null, originatorTypeName, ownerType.FullName);
        }

        private static bool TypeNameMatches(Type originatorType, string originatorTypeName, string ownerTypeName)
        {
            if (originatorType != null)
            {
                if (originatorType.Name == ownerTypeName || originatorType.FullName == ownerTypeName) return true;
            }

            if (originatorTypeName == null) return false;

            return originatorTypeName == ownerTypeName
                || originatorTypeName.EndsWith("." + ownerTypeName, StringComparison.Ordinal)
                || originatorTypeName.EndsWith(":" + ownerTypeName, StringComparison.Ordinal);
        }

        private static string GetOriginatorTypeName(object eventEntry)
        {
            FieldInfo originatorTypeField = AccessTools.Field(eventEntry.GetType(), "originatorType");
            return originatorTypeField != null ? originatorTypeField.GetValue(eventEntry) as string : null;
        }

        internal static object GetInstanceField(object source, string fieldName)
        {
            if (source == null) return null;
            FieldInfo field = AccessTools.Field(source.GetType(), fieldName);
            return field != null ? field.GetValue(source) : null;
        }

        internal static object GetStaticField(Type type, string fieldName)
        {
            if (type == null) return null;
            FieldInfo field = type.GetField(fieldName, AnyStatic);
            return field != null ? field.GetValue(null) : null;
        }

        internal static object GetStaticMember(Type type, string memberName)
        {
            if (type == null) return null;

            PropertyInfo property = type.GetProperty(memberName, AnyStatic);
            if (property != null) return property.GetValue(null, null);

            FieldInfo field = type.GetField(memberName, AnyStatic);
            return field != null ? field.GetValue(null) : null;
        }

        private static int GetEventEntryCount(object eventSource)
        {
            if (eventSource == null) return -1;

            FieldInfo eventsField = AccessTools.Field(eventSource.GetType(), "events");
            if (eventsField == null) return -1;

            IList events = eventsField.GetValue(eventSource) as IList;
            return events != null ? events.Count : -1;
        }

        private static string DescribeEventSource(object eventSource)
        {
            if (eventSource == null) return "<null>";

            BaseGameEvent gameEvent = eventSource as BaseGameEvent;
            if (gameEvent != null) return gameEvent.EventName;

            Type type = eventSource.GetType();
            return type.FullName ?? type.Name;
        }

        private static string FormatTypeName(Type type)
        {
            if (type == null) return "<null>";

            string assemblyName = type.Assembly.GetName().Name;
            if (type.IsNested && type.DeclaringType != null)
                return assemblyName + ":" + type.DeclaringType.Name + "." + type.Name;

            return assemblyName + ":" + type.Name;
        }

        private static void LogRemoval(string strategy, string eventName, Type ownerType, int removed)
        {
            LogRemoval(strategy, eventName, FormatTypeName(ownerType), removed);
        }

        private static void LogRemoval(string strategy, string eventName, string ownerTypeName, int removed)
        {
            if (!NoMoreLeaksSettings.VerboseDebugLogging || removed <= 0) return;

            Debug.Log("[NoMoreLeaks:Debug] Removed " + removed + " callback(s) from " + eventName + " owned by " + ownerTypeName + " via " + strategy);
        }

        private static MethodInfo FindSingleDelegateParameterMethod(Type type, string name)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.Name != name) continue;
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && typeof(Delegate).IsAssignableFrom(parameters[0].ParameterType))
                    return method;
            }

            return null;
        }

        private static MethodInfo FindInstanceMethod(Type type, string name)
        {
            while (type != null)
            {
                MethodInfo method = type.GetMethod(name, AnyInstance);
                if (method != null) return method;
                type = type.BaseType;
            }

            return null;
        }
    }
}
