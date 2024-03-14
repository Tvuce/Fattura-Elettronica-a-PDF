using FatEle2PDF.Properties;

namespace FatEle2PDF.Classes
{
    public class XML
    {
        private string _filePath;
        private string _RSoc;
        private DateOnly _dDoc;
        private string _pIVA;
        private double _totImp;

        public string filePath => _filePath;
        public string RSoc => _RSoc;
        public DateOnly dDoc => _dDoc;
        public string pIVA => _pIVA;
        public double totImp => _totImp;

        public XML(string filePath, string rsoc, DateOnly dDoc, string pIva,  double totImp)
        {
            _filePath = filePath;
            _RSoc = rsoc;
            _dDoc = dDoc;
            _pIVA = pIva;
            _totImp = totImp;
        }

    }
    public class RSocComparerAsc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            if (x.RSoc == y.RSoc)
            {
                return x.dDoc.CompareTo(y.dDoc);
            }
            else
            {
                return x.RSoc.CompareTo(y.RSoc);
            }
        }
    }
    public class RSocComparerDesc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            if (x.RSoc == y.RSoc)
            {
                return x.dDoc.CompareTo(y.dDoc);
            }
            else
            {
                return y.RSoc.CompareTo(x.RSoc);
            }
        }
    }
    public class DDocComparerAsc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            return x.dDoc.CompareTo(y.dDoc);
        }
    }

    public class DDocComparerDesc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            return y.dDoc.CompareTo(x.dDoc);
        }
    }

    public class PIvaComparerAsc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            if (x.pIVA == y.pIVA)
            {
                return x.dDoc.CompareTo(y.dDoc);
            }
            else
            {
                return x.pIVA.CompareTo(y.pIVA);
            }
        }
    }
    public class PIvaComparerDesc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            if (x.pIVA == y.pIVA)
            {
                return x.dDoc.CompareTo(y.dDoc);
            }
            else
            {
                return y.pIVA.CompareTo(x.pIVA);
            }
        }
    }
    public class TotImpComparerAsc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            if (x.totImp == y.totImp)
            {
                return x.dDoc.CompareTo(y.dDoc);
            }
            else
            {
                return x.totImp.CompareTo(y.totImp);
            }
        }
    }
    public class TotImpComparerDesc : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            if (x.totImp == y.totImp)
            {
                return x.dDoc.CompareTo(y.dDoc);
            }
            else
            {
                return y.totImp.CompareTo(x.totImp);
            }
        }
    }
}
