using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockerRentalManagementSystem
{
    class Page
    {
        private const int _maxItems = 100;
        private int _pageNumber;
        private int _indexLimit;
        private int _firstIndex;
        private int _lastIndex;
        private int _lastPage;
        private double _finalIndex;

        public int MaxItems { get { return _maxItems; } }
        public int PageNumber { get { return _pageNumber; } set { _pageNumber = value; } }
        public int IndexLimit { get { return _indexLimit; } set { _indexLimit = value; } }
        public int FirstIndex { get { return _firstIndex; } set { _firstIndex = value; } }
        public int LastIndex { get { return _lastIndex; } set { _lastIndex = value; } }
        public int LastPage { get { return _lastPage; } set { _lastPage = value; } }
        public double FinalIndex { get { return _finalIndex; } set { _finalIndex = value; } }

        public Page()
        {
            _pageNumber = 1;
            _indexLimit = 0;
            _firstIndex = 0;
            _lastIndex = 0;
            _lastPage = 1;
            _finalIndex = 0;
        }

        public void PageReset()
        {
            _pageNumber = 1;
            _indexLimit = 0;
            _firstIndex = 0;
            _lastIndex = 0;
            _lastPage = 1;
            _finalIndex = 0;

        }

        public void PageSetting()
        {
            if (_pageNumber > _lastPage)
            {
                _pageNumber = _lastPage;
            }
            _indexLimit = (_pageNumber - 1) * _maxItems;
            _firstIndex = _indexLimit + 1;
            _lastIndex = _pageNumber * _maxItems;
        }
    }
}
