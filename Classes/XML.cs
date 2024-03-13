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
    public class RSocComparer : IComparer<XML>
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

    public class DDocComparer : IComparer<XML>
    {
        public int Compare(XML? x, XML? y)
        {
            return x.dDoc.CompareTo(y.dDoc);
        }
    }

    public class PIvaComparer : IComparer<XML>
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

    public class TotImpComparer : IComparer<XML>
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
}
