using System.Collections;

namespace Array2D
{
    //Type parameter T in angle brackets.
    public class List2D<T> : System.Collections.Generic.IEnumerable<T>
    {
        private int maxX = -1;
        private int maxY = -1;
        public Hashtable hashTable = new Hashtable();
        private bool Empty = true;
        private object nil = null;
        private object getvalue(int x, int y)
        {
            string key = x.ToString() + "@" + y.ToString();
            if (!hashTable.Contains(key)) return nil;
            return hashTable[key];
        }
        private void setvalue(int x, int y, object value)
        {
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            string key = x.ToString() + "@" + y.ToString();
            if (hashTable.Contains(key))
            {
                hashTable[key] = value;
            }
            else
            {
                hashTable.Add(key, value);
            }
            Empty = false;
        }

        public int MaxX
        {
            get { return maxX; }
            set { maxX = value; }
        }
        public int MaxY
        {
            get { return maxY; }
            set { maxY = value; }
        }
        public bool IsEmpty
        {
            get { return Empty; }
        }
        public void Clear()
        {
            hashTable.Clear();
            maxX = -1;
            maxY = -1;
            Empty = true;
        }

        public T this[int indexX, int indexY]
        {
            set { setvalue(indexX, indexY, (object)value); }
            get { return (T)getvalue(indexX, indexY); }
        }

        protected Node head;
        protected Node current = null;

        // Nested class is also generic on T
        protected class Node
        {
            public Node next;
            private T data;  //T as private member datatype

            public Node(T t)  //T used in non-generic constructor
            {
                next = null;
                data = t;
            }

            public Node Next
            {
                get { return next; }
                set { next = value; }
            }

            public T Data  //T as return type of property
            {
                get { return data; }
                set { data = value; }
            }
        }

        public List2D()  //constructor
        {
            head = null;
        }

        public void AddHead(T t)  //T as method parameter type
        {
            Node n = new Node(t);
            n.Next = head;
            head = n;
        }

        // Implementation of the iterator
        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            Node current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }

        // IEnumerable<T> inherits from IEnumerable, therefore this class 
        // must implement both the generic and non-generic versions of 
        // GetEnumerator. In most cases, the non-generic method can 
        // simply call the generic method.
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Char2D
    {
        private int maxX=-1;
        private int maxY=-1;
        public Hashtable hashTable=new Hashtable();
        private bool Empty = true;
        private object nil = '\0';

        public Hashtable GetAllHash
        {
            get { return hashTable; }
        }
        private object getvalue(int x,int y)
        {
            string key = x.ToString() + "@" + y.ToString();
            if (!hashTable.Contains(key)) return nil;
            return hashTable[key];
        }
        private void setvalue(int x, int y,object value)
        {
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            string key = x.ToString() + "@" + y.ToString();
            if (hashTable.Contains(key))
            {
                hashTable[key] = value;
            }
            else
            {
                hashTable.Add(key, value);
            }
            Empty = false;
        }

        public int MaxX
        {
            get { return maxX; }
            set { maxX = value; }
        }
        public int MaxY
        {
            get { return maxY; }
            set { maxY = value; }
        }
        public bool IsEmpty
        {
            get {return Empty;}
        }
        public void Clear()
        {
            hashTable.Clear();
            maxX = -1;
            maxY = -1;
            Empty = true;
        }

        public char this[int indexX,int indexY]
        {
            set { setvalue(indexX, indexY, (object)value); }
            get { return (char)getvalue(indexX, indexY); }
        }
    }
}