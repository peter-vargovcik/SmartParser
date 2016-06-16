using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace SmartParser
{
    class MyLinkedList<NPOI_Node> : IEnumerable<NPOI_Node> where NPOI_Node : IRow
    {
        public class Node
        {
            public NPOI_Node NodeContent;
            public string hash;
            public Node Previous;
            public Node Next;
            public bool isHeader = false;
        }

        private int size;
        public int Count
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// The head of the list.
        /// </summary>
        private Node head;

        /// <summary>
        /// The current node, used to avoid adding nodes before the head
        /// </summary>
        private Node current;

        public MyLinkedList()
        {
            size = 0;
            head = null;
        }


        /// <summary>
        /// Add a new Node to the list.
        /// </summary>
        public void Add(NPOI_Node content, string _hash)
        {
            size++;
            Node tempCurrent = current;

            // This is a more verbose implementation to avoid adding nodes to the head of the list
            var node = new Node()
            {
                NodeContent = content,
                hash = _hash
            };

            if (head == null)
            {
                // This is the first node. Make it the head
                head = node;
            }
            else
            {
                // This is not the head. Make it current's next node.
                current.Next = node;
                node.Previous = tempCurrent;
            }

            // Makes newly added node the current node
            current = node;


            // This implementation is simpler but adds nodes in reverse order. It adds nodes to the head of the list

            //head = new Node()
            //{
            //    Next = head,
            //    NodeContent = content
            //};

        }

        /// <summary>
        ///  Throwing this in to help test the list
        /// </summary>
        public void ListNodes()
        {
            Node tempNode = head;

            while (tempNode != null)
            {
                Console.WriteLine(tempNode.NodeContent);
                tempNode = tempNode.Next;
            }
        }



        /// <summary>
        /// Returns the Node in the specified position or null if inexistent
        /// </summary>
        /// <param name="Position">One based position of the node to retrieve</param>
        /// <returns>The desired node or null if inexistent</returns>
        public Node Retrieve(int Position)
        {
            Node tempNode = head;
            Node retNode = null;
            int count = 0;

            while (tempNode != null)
            {
                if (count == Position - 1)
                {
                    retNode = tempNode;
                    break;
                }
                count++;
                tempNode = tempNode.Next;
            }

            return retNode;
        }

        /// <summary>
        /// Delete a Node in the specified position
        /// </summary>
        /// <param name="Position">Position of node to be deleted</param>
        /// <returns>Successful</returns>
        public bool Delete(int Position)
        {
            if (Position == 1)
            {
                head = null;
                current = null;
                return true;
            }

            if (Position > 1 && Position <= size)
            {
                Node tempNode = head;

                Node lastNode = null;
                int count = 0;

                while (tempNode != null)
                {
                    if (count == Position - 1)
                    {
                        lastNode.Next = tempNode.Next;
                        return true;
                    }
                    count++;

                    lastNode = tempNode;
                    tempNode = tempNode.Next;
                }
            }

            return false;
        }

        internal void evaluate()
        {
            Node tempNode = head;
            Node previous = null;
            Node next = null;

            while (tempNode != null)
            {
                previous = tempNode.Previous;
                next = tempNode.Next;
                _evalNodes(previous, tempNode, next);
                tempNode = tempNode.Next;
            }
        }

        private void _evalNodes(Node previous, Node tempNode, Node next)
        {
            if(previous == null && _hashNotSame(tempNode.hash, next.hash))
            {
                tempNode.isHeader = true;
            }

            //tempNode.NodeContent.
        }

        private bool _hashNotSame(string previousHash, string currentHashRow)
        {
            StringComparer stringComparer = StringComparer.InvariantCulture;

            if (stringComparer.Compare(previousHash, currentHashRow) == 0)
                return false;
            else
                return true;
        }

        public IEnumerator<NPOI_Node> GetEnumerator()
        {
            Node tempNode = head;

            while (tempNode != null)
            {
                yield return (NPOI_Node) tempNode.NodeContent;
                tempNode = tempNode.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Node tempNode = head;

            while (tempNode != null)
            {
                yield return tempNode.NodeContent;
                tempNode = tempNode.Next;
            }
        }
    }
}
