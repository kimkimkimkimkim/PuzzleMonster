using System;
using System.Collections.Generic;

internal class CachedRecords<T> where T : MasterBookBase
{
    private readonly List<T> _list;
    private readonly Dictionary<long, T> _book;

    public CachedRecords(MasterContentContainer<T> container)
    {
        _list = new List<T>(container.BookList);
        _book = new Dictionary<long, T>();
        foreach (var record in _list)
        {
            _book.Add(record.id, record);
        }
    }

    public CachedRecords(IEnumerable<T> arr)
    {
        _list = new List<T>(arr);
        _book = new Dictionary<long, T>();
        foreach (var record in _list)
        {
            _book.Add(record.id, record);
        }
    }

    public int Count
    {
        get
        {
            return _list.Count;
        }
    }

    public T Get(long id)
    {
        return !_book.ContainsKey(id) ? null : _book[id];
    }

    public T GetAt(int index)
    {
        return index < _list.Count ? _list[index] : null;
    }

    public T Find(Predicate<T> match)
    {
        return _list.Find(match);
    }

    public List<T> FindAll(Predicate<T> match)
    {
        return _list.FindAll(match);
    }

    public IEnumerable<T> GetAll()
    {
        return _list;
    }
}