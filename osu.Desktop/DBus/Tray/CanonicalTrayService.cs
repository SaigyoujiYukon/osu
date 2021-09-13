using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M.DBus.Services.Canonical;
using M.DBus.Tray;
using M.DBus.Utils.Canonical.DBusMenuFlags;
using osu.Framework.Logging;
using Tmds.DBus;
using Logger = osu.Framework.Logging.Logger;

namespace osu.Desktop.DBus.Tray
{
    /// <summary>
    /// todo: 找到文档并实现所有目前未实现的功能
    /// https://github.com/gnustep/libs-dbuskit/blob/master/Bundles/DBusMenu/com.canonical.dbusmenu.xml
    /// </summary>
    public class CanonicalTrayService : IDBusMenu
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/MenuBar");

        #region Canonical DBus

        private readonly DBusMenuProperties canonicalProperties = new DBusMenuProperties();

        private uint menuRevision;

        private readonly RootEntry rootEntry = new RootEntry
        {
            ChildrenDisplay = ChildrenDisplayType.SSubmenu
        };

        #region 列表物件存储

        private readonly Dictionary<int, SimpleEntry> entries = new Dictionary<int, SimpleEntry>();

        private int lastEntryId = 1;

        private void addEntry(SimpleEntry entry)
        {
            if (entry is RootEntry)
                throw new InvalidOperationException("不能添加多个RootEntry");

            rootEntry.Children.Add(entry);
            entries[lastEntryId] = entry;
            lastEntryId++;
        }

        public void AddEntryRange(SimpleEntry[] entries)
        {
            foreach (var entry in entries)
            {
                addEntry(entry);
            }

            triggerLayoutUpdate();
        }

        public void AddEntryToMenu(SimpleEntry entry)
        {
            addEntry(entry);
            triggerLayoutUpdate();
        }

        public void RemoveEntryFromMenu(SimpleEntry entry)
        {
            var key = entries.FirstOrDefault(p => p.Value == entry);

            if (key.Value != null)
            {
                entries.Remove(key.Key);
                rootEntry.Children.Remove(key.Value);
            }
            else
                throw new InvalidOperationException($"给定的 {entry} 不在列表中");

            triggerLayoutUpdate();
        }

        private void triggerLayoutUpdate()
        {
            menuRevision++;
            menuChanged = true;
            OnLayoutUpdated?.Invoke((menuRevision, 0));
        }

        #endregion

        private int dbusItemMaxOrder;
        private bool menuChanged;

        public Task<(uint revision, (int, IDictionary<string, object>, object[]) layout)> GetLayoutAsync(int parentId, int recursionDepth, string[] propertyNames)
        {
            Logger.Log($"方法被调用：GetLayoutAsync: parentId: {parentId} | recursionDepth: {recursionDepth}", level: LogLevel.Verbose);

            int addit;

            var result =
                (menuRevision, rootEntry.ToDbusObject(
                    rootEntry.ChildId,
                    dbusItemMaxOrder,
                    out addit));

            dbusItemMaxOrder += addit;

            return Task.FromResult(result);
        }

        public Task<(int, IDictionary<string, object>)[]> GetGroupPropertiesAsync(int[] ids, string[] propertyNames)
        {
            Logger.Log("方法被调用：GetGroupPropertiesAsync: ids:", level: LogLevel.Verbose);

            foreach (var id in ids)
            {
                Logger.Log(id.ToString());
            }

            var result = new List<(int, IDictionary<string, object>)>();

            foreach (var id in ids)
            {
                var target = entries.FirstOrDefault(k => k.Key == id);
                if (target.Value == null) continue;

                result.Add(target.Value.ToDbusObject());
            }

            return Task.FromResult(result.ToArray());
        }

        public Task<object> GetPropertyAsync(int id, string name)
        {
            Logger.Log($"未实现的方法被调用：GetPropertyAsync: id: {id} | name: {name}");
            throw new NotImplementedException();
        }

        public Task EventAsync(int id, string eventId, object data, uint timestamp)
        {
            var eventType = eventId.ToEventType();

            switch (eventType)
            {
                case EventType.Clicked:
                    entries.FirstOrDefault(p => p.Key == id).Value?.OnActive?.Invoke();
                    break;

                case EventType.Closed:
                    break;

                case EventType.Opened:
                    break;

                default:
                    Logger.Log($"未实现的方法被调用：EventAsync: id: {id} | eventId: {eventId} | data: {data} | timestamp: {timestamp}");
                    break;
            }

            return Task.CompletedTask;
        }

        public Task<int[]> EventGroupAsync((int, string, object, uint)[] events)
        {
            Logger.Log($"未实现的方法被调用：EventGroupAsync: {events}");
            throw new NotImplementedException();
        }

        public Task<bool> AboutToShowAsync(int id)
        {
            Logger.Log("方法被调用: AboutToShowAsync", level: LogLevel.Verbose);

            bool returnValue = false;

            if (id == rootEntry.ChildId)
            {
                returnValue = menuChanged;
                menuChanged = false;
            }

            return Task.FromResult(returnValue);
        }

        public Task<(int[] updatesNeeded, int[] idErrors)> AboutToShowGroupAsync(int[] ids)
        {
            Logger.Log($"未实现的方法被调用：AboutToShowGroupAsync: {ids}");
            throw new NotImplementedException();
        }

        public event Action<((int, IDictionary<string, object>)[] updatedProps, (int, string[])[] removedProps)> OnEntriesUpdated;

        public Task<IDisposable> WatchItemsPropertiesUpdatedAsync(Action<((int, IDictionary<string, object>)[] updatedProps, (int, string[])[] removedProps)> handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnEntriesUpdated), handler);
        }

        public event Action<(uint revision, int parent)> OnLayoutUpdated;

        public Task<IDisposable> WatchLayoutUpdatedAsync(Action<(uint revision, int parent)> handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnLayoutUpdated), handler);
        }

        public event Action<(int id, uint timestamp)> OnItemActivationRequested;

        public Task<IDisposable> WatchItemActivationRequestedAsync(Action<(int id, uint timestamp)> handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnItemActivationRequested), handler);
        }

        public Task<object> GetAsync(string prop)
        {
            return Task.FromResult(canonicalProperties.Get(prop));
        }

        public Task<DBusMenuProperties> GetAllAsync()
        {
            return Task.FromResult(canonicalProperties);
        }

        public Task SetAsync(string prop, object val)
        {
            throw new NotImplementedException();
        }

        internal bool SetProperty(string prop, object value)
            => canonicalProperties.Set(prop, value);

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
        }

        public Task<uint> GetVersionAsync()
        {
            return Task.FromResult(canonicalProperties.Version);
        }

        public Task<string> GetTextDirectionAsync()
        {
            return Task.FromResult(canonicalProperties.TextDirection);
        }

        public Task<string> GetStatusAsync()
            => Task.FromResult(canonicalProperties.Status);

        Task<string[]> IDBusMenu.GetIconThemePathAsync()
            => Task.FromResult(canonicalProperties.IconThemePath);

        #endregion
    }
}
