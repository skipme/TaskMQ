using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace SecQueue
{
    public class IndexElement
    {
        public TaskQueue.ValueMap<string, object> IndexKey;
        public int Reference;

        public IndexElement(TaskQueue.ValueMap<string, object> ikey)
        {
            IndexKey = ikey;
        }
    }
    public class IndexSector
    {
        public int Location;
        public List<IndexElement> IndexElements;
        IComparer<TaskQueue.ValueMap<string, object>> comparator;

        public int TopGradientReference;
        public bool IsSerial;

        public IndexSector(IComparer<TaskQueue.ValueMap<string, object>> comparator, bool isSerial = true)
        {
            Location = -1;
            IsSerial = isSerial;
            this.comparator = comparator;
        }

        // perform a binary search on array
        public int BinarySearch(TaskQueue.ValueMap<string, object> searchElement, out int cmp)
        {
            if (IndexElements.Count == 0) { cmp = 0; return 0; }

            int low = 0; // 0 is always going to be the first element
            int high = IndexElements.Count - 1; // Find highest element
            int middle = (low + high + 1) / 2; // Find middle element

            do // Search for element
            {
                cmp = comparator.Compare(searchElement, IndexElements[middle].IndexKey);
                if (cmp == 0)
                {
                    return middle;
                }

                // middle element is too high
                else //if (searchElement < data[middle])
                    if (cmp < 0)
                        high = middle - 1; // eliminate top half
                    else // middle element is too low
                        low = middle + 1; // eleminate bottom half

                middle = (low + high + 1) / 2; // recalculate the middle  
            } while ((low <= high)/* && (location == -1)*/);

            return middle;// nearest
        }
    }
    public class SecSortedSet
    {
        public int IndexMaxElementsCount;
        public int GradMaxElementsCount;

        public List<IndexSector> AllSectors;
        // 
        public IndexSector RootGradient;// this gradient contains all records
        public IndexSector initialSector;// first index sector
        public Queue<int> FreeSectors;

        const int ROOTGRADIENT = 0xFFEFE;
        const int MINSECTORCOUNT = 2;

        const int MAXGRADIENTLEVEL = 1000;// total elements = n^MAXGRADIENTLEVEL
        IComparer<TaskQueue.ValueMap<string, object>> comparator;

        private int keysCount = 0;
        public int Count
        {
            get
            {
                return keysCount;
            }
        }
        public SecSortedSet(IComparer<TaskQueue.ValueMap<string, object>> comparator, int sectorCountSeria = 24, int sectorCountGradient = 24)
        {
            //if (MINSECTORCOUNT < 2)
            //    throw new Exception("!!!!!!!!!!!!");
            this.comparator = comparator;
            IndexMaxElementsCount = sectorCountSeria;

            GradMaxElementsCount = sectorCountGradient;

            if (IndexMaxElementsCount < MINSECTORCOUNT || GradMaxElementsCount < MINSECTORCOUNT)
                throw new Exception("sector size is too small");

            AllSectors = new List<IndexSector>();

            RootGradient = new IndexSector(comparator, false) { TopGradientReference = ROOTGRADIENT };
            initialSector = new IndexSector(comparator, true);
            // TODO: REREAD ROOT GRADIENT
            RootGradient.IndexElements = new List<IndexElement>();
            initialSector.IndexElements = new List<IndexElement>();

            FreeSectors = new Queue<int>();
            // ~~~~
            SetSector(initialSector);
            SetSector(RootGradient);
            initialSector.TopGradientReference = RootGradient.Location;
        }

        private void SetSector(IndexSector sector)
        {
            if (sector.Location < 0)
            {
                if (FreeSectors.Count > 0)
                {
                    int freesec = FreeSectors.Dequeue();
                    sector.Location = freesec;
                    AllSectors[freesec] = sector;
                }
                else
                {
                    sector.Location = AllSectors.Count;
                }
            }
            else return;
            AllSectors.Add(sector);
        }
        private IndexSector GetSector(int Reference)
        {
            return AllSectors[Reference];
        }
        public int Search(TaskQueue.ValueMap<string, object> key)
        {
            int outCmp = 0, idx;
            IndexSector currentSector = RootGradient;
            for (int i = 0; i < 1000; i++)
            {
                idx = currentSector.BinarySearch(key, out outCmp);
                IndexElement el = currentSector.IndexElements[idx];
                if (currentSector.IsSerial)
                {
		    if(outCmp != 0)
		    {
			return -1; // not found
	            }
                    return idx;
                }
                else
                {
                    currentSector = GetSector(el.Reference);
                }
            }
            return -1; // not found
        }

        private enum QOperationType
        {
            ADDKEY,
            UPDATEGRADIENTKEY
        }
        private class QOperation
        {
            public QOperationType OpType;
            public IndexSector Reference;
            public TaskQueue.ValueMap<string, object> Key;
            public int index = -1;
            public int location;
        }

        public void Add(TaskQueue.ValueMap<string, object> key)
        {
            int outCmp = 0, idx;
            if (RootGradient.IndexElements.Count == 0)
            {
                initialSector.IndexElements.Add(new IndexElement(key));
                RootGradient.IndexElements.Add(new IndexElement(key) { Reference = initialSector.Location });
                keysCount++;
                return;
            }
            IndexSector currentSector = RootGradient;
            for (int i = 0; i < MAXGRADIENTLEVEL; i++)
            {
                idx = currentSector.BinarySearch(key, out outCmp);
                if (currentSector.IsSerial)
                {
                    if (outCmp == 0 && currentSector.IndexElements.Count > 0)
                    {
                        // duplicates not allowed
                        throw new Exception("duplicates not allowed");
                        // actually allowed, but in this way remove operation will not perform correctly 
                    }

                    Queue<QOperation> operationsQueue = new Queue<QOperation>();
                    operationsQueue.Enqueue(new QOperation()
                    {
                        Key = key,
                        OpType = QOperationType.ADDKEY,
                        Reference = currentSector,
                        index = idx
                    });
                    while (operationsQueue.Count > 0)
                    {
                        QOperation op = operationsQueue.Dequeue();
                        switch (op.OpType)
                        {
                            case QOperationType.ADDKEY:
                                if (op.index < 0)
                                {
                                    op.index = op.Reference.BinarySearch(op.Key, out outCmp);
                                }
                                IndexElement new_element;
                                if (op.Reference.IsSerial)
                                {
                                    new_element = new IndexElement(op.Key);
                                }
                                else
                                    new_element = new IndexElement(op.Key) { Reference = op.location };


                                if (op.Reference.IndexElements.Count == (op.Reference.IsSerial ? IndexMaxElementsCount : GradMaxElementsCount))
                                {
                                    if (!op.Reference.IsSerial && op.Reference.TopGradientReference == ROOTGRADIENT)
                                    {
                                        // allocate new root gradient
                                        IndexSector new_root_sector = new IndexSector(comparator, false);
                                        new_root_sector.IndexElements = new List<IndexElement>();
                                        new_root_sector.IndexElements.Add(new IndexElement(op.Reference.IndexElements[0].IndexKey) { Reference = op.Reference.Location });
                                        new_root_sector.TopGradientReference = ROOTGRADIENT;

                                        SetSector(new_root_sector);
                                        op.Reference.TopGradientReference = new_root_sector.Location;
                                        RootGradient = new_root_sector;
                                    }
                                    IndexSector grad = GetSector(op.Reference.TopGradientReference);
                                    // overflow
                                    IndexSector new_sector = new IndexSector(comparator, op.Reference.IsSerial);
                                    new_sector.IndexElements = new List<IndexElement>();
                                    new_sector.TopGradientReference = op.Reference.TopGradientReference;
                                    // split....
                                    if (op.index != op.Reference.IndexElements.Count)
                                    {
                                        //for (int j = op.index; j < op.Reference.IndexElements.Count; j++)
                                        //{
                                        //    if (comparator.Compare(op.Key, op.Reference.IndexElements[j].IndexKey) == 0) op.index++;
                                        //}
                                        op.Reference.IndexElements.Insert(op.index, new_element);
                                        int cutindex = op.Reference.IndexElements.Count / 2;
                                        new_sector.IndexElements = op.Reference.IndexElements.GetRange(cutindex, op.Reference.IndexElements.Count - cutindex);
                                        op.Reference.IndexElements.RemoveRange(cutindex, op.Reference.IndexElements.Count - cutindex);
                                        SetSector(new_sector);
                                        if (op.index == 0)
                                        {
                                            operationsQueue.Enqueue(new QOperation()
                                            {
                                                OpType = QOperationType.UPDATEGRADIENTKEY,
                                                Key = key,
                                                Reference = grad,
                                                location = op.Reference.Location
                                            });
                                        }
                                        if (!op.Reference.IsSerial)
                                        {
                                            for (int j = 0; j < new_sector.IndexElements.Count; j++)
                                            {
                                                IndexSector reftoup = GetSector(new_sector.IndexElements[j].Reference);
                                                reftoup.TopGradientReference = new_sector.Location;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // split not required             
                                        new_sector.IndexElements.Add(new_element);
                                        SetSector(new_sector);
                                        if (!new_sector.IsSerial)
                                        {
                                            IndexSector reftoup = GetSector(new_element.Reference);
                                            reftoup.TopGradientReference = new_sector.Location;
                                        }
                                    }

                                    operationsQueue.Enqueue(new QOperation()
                                    {
                                        OpType = QOperationType.ADDKEY,
                                        Key = new_sector.IndexElements[0].IndexKey,
                                        Reference = grad,
                                        index = -1,
                                        location = new_sector.Location
                                    });
                                }
                                else
                                {
                                    // just add element
                                    //if (op.index == 0)
                                    //    for (int j = op.index; j < op.Reference.IndexElements.Count; j++)
                                    //    {
                                    //        if (comparator.Compare(op.Key, op.Reference.IndexElements[j].IndexKey) == 0) op.index++;
                                    //    }
                                    op.Reference.IndexElements.Insert(op.index, new_element);
                                    if ((op.index == 0 || op.Reference.IndexElements.Count == 1) && op.Reference.TopGradientReference != ROOTGRADIENT)
                                    {
                                        operationsQueue.Enqueue(new QOperation()
                                        {
                                            OpType = QOperationType.UPDATEGRADIENTKEY,
                                            Key = new_element.IndexKey,
                                            Reference = GetSector(op.Reference.TopGradientReference),
                                            location = op.Reference.Location
                                        });
                                    }
                                }
                                break;
                            case QOperationType.UPDATEGRADIENTKEY:
                                for (int j = 0; j < op.Reference.IndexElements.Count; j++)
                                {
                                    if (op.Reference.IndexElements[j].Reference == op.location)
                                    {
                                        op.Reference.IndexElements[j].IndexKey = op.Key;

                                        if (j == 0 && op.Reference.TopGradientReference != ROOTGRADIENT)
                                        {
                                            operationsQueue.Enqueue(new QOperation()
                                            {
                                                OpType = QOperationType.UPDATEGRADIENTKEY,
                                                Key = op.Key,
                                                Reference = GetSector(op.Reference.TopGradientReference),
                                                location = op.Reference.Location
                                            });
                                        }
                                        break;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    //--------------
                    break;
                }
                else
                {
                    IndexElement el = null;
                    if (outCmp == 0)
                        el = currentSector.IndexElements[idx];
                    else
                        el = currentSector.IndexElements[idx > 0 ? idx - 1 : idx];

                    currentSector = GetSector(el.Reference);
                    // go deeper
                }
            }
            keysCount++;
        }
        public bool Remove(TaskQueue.ValueMap<string, object> key)
        {
            int outCmp = 0, idx;

            IndexSector currentSector = RootGradient;
            for (int i = 0; i < MAXGRADIENTLEVEL; i++)
            {
                idx = currentSector.BinarySearch(key, out outCmp);

                if (currentSector.IsSerial)
                {
                    if (outCmp == 0)
                    {
                        // ok we found it, remove it now
                        currentSector.IndexElements.RemoveAt(idx);
                        if (currentSector.IndexElements.Count == 0)
                        {
                            // nothing to do with it, sector remains with references on it in gradients

                            // or we can remove it if storage in-memory...
                            TaskQueue.ValueMap<string, object> newKey = null;
                            IndexSector refSector = currentSector;
                            IndexSector grad = GetSector(currentSector.TopGradientReference);
                            bool elementFound = true, nextElement = false, removeElement = true;
                            while (true)
                            {
                                elementFound = nextElement = false;
                                for (int j = 0; j < grad.IndexElements.Count; j++)
                                {
                                    if (grad.IndexElements[j].Reference == refSector.Location)
                                    {
                                        elementFound = true;
                                        if (removeElement)
                                        {
                                            if (grad.TopGradientReference != ROOTGRADIENT || grad.IndexElements.Count > 1)
                                                grad.IndexElements.RemoveAt(j);
                                        }
                                        else
                                        {
                                            // update key if key in previous sector located at 0 index
                                            grad.IndexElements[j].IndexKey = newKey;
                                        }
                                        if (grad.IndexElements.Count == 0 && grad.TopGradientReference != ROOTGRADIENT)
                                        {
                                            FreeSectors.Enqueue(grad.Location);
                                            AllSectors[grad.Location] = null;
                                            removeElement = true;
                                            refSector = grad;
                                            grad = GetSector(grad.TopGradientReference);
                                            nextElement = true;
                                        }
                                        else
                                            if (j == 0 && grad.TopGradientReference != ROOTGRADIENT)
                                            {
                                                removeElement = false;
                                                newKey = grad.IndexElements[j].IndexKey;
                                                refSector = grad;
                                                grad = GetSector(grad.TopGradientReference);
                                                nextElement = true;
                                            }
                                        break;// 
                                    }
                                }
                                if (!elementFound)
                                {
                                    throw new Exception("reference key to sector not found");
                                }
                                else if (!nextElement)
                                {
                                    break;
                                }
                            }
                            keysCount--;
                            return true;
                        }
                        else
                            if (idx == 0)
                            {
                                // update ranges
                                TaskQueue.ValueMap<string, object> newKey = currentSector.IndexElements[0].IndexKey;
                                IndexSector refSector = currentSector;
                                IndexSector grad = GetSector(currentSector.TopGradientReference);
                                bool elementFound = true, nextElement = false;
                                while (true)
                                {
                                    elementFound = nextElement = false;
                                    for (int j = 0; j < grad.IndexElements.Count; j++)
                                    {
                                        if (grad.IndexElements[j].Reference == refSector.Location)
                                        {
                                            elementFound = true;
                                            grad.IndexElements[j].IndexKey = newKey;
                                            if (j == 0 && grad.TopGradientReference != ROOTGRADIENT)
                                            {
                                                refSector = grad;
                                                grad = GetSector(grad.TopGradientReference);
                                                nextElement = true;
                                            }
                                            break;// 
                                        }
                                    }
                                    if (!elementFound)
                                    {
                                        throw new Exception("reference key to sector not found");
                                    }
                                    else if (!nextElement)
                                    {
                                        break;
                                    }
                                }
                                // removed successfully
                                keysCount--;
                                return true;
                            }
                    }
                    else
                    {
                        // not found
                        return false;
                    }
                }
                else
                {
                    IndexElement el = null;
                    if (outCmp == 0)
                        el = currentSector.IndexElements[idx];
                    else
                        el = currentSector.IndexElements[idx > 0 ? idx - 1 : idx];

                    currentSector = GetSector(el.Reference);
                    // go deeper
                }
            }
            // not found
            return false;
        }

        public IEnumerable<IndexElement> EnumerateKeys()
        {
            List<int> subCounters = new List<int>() { 0 };
            Stack<IndexSector> sectorStack = new Stack<IndexSector>();
            int currentLevel = 0;// current Gradient Level < MAXGRADIENTLEVEL 

            IndexSector currentSector = RootGradient;
            while (true)
            {
                int idx = subCounters[currentLevel];
                if (currentSector.IndexElements.Count == idx)
                {
                    subCounters[currentLevel] = 0;
                    currentLevel--;
                    if (currentLevel < 0) break;
                    currentSector = sectorStack.Pop();
                    subCounters[currentLevel]++;
                    continue;
                }
                IndexElement el = currentSector.IndexElements[idx];
                if (currentSector.IsSerial)
                {
                    yield return el;
                    subCounters[currentLevel] = idx + 1;
                }
                else
                {
                    sectorStack.Push(currentSector);
                    currentSector = GetSector(el.Reference);
                    currentLevel++;
                    if (subCounters.Count == currentLevel)
                        subCounters.Add(0);
                }
            }
        }
        public TaskQueue.ValueMap<string, object> GetMinKey()
        {
            IndexElement il = GetMinWithoutLeftBehindSectors();
            return il == null ? null : il.IndexKey;
        }

        private IndexElement GetMinWithoutLeftBehindSectors()
        {
            // allowed only with if empty sectors removed from gradients
            IndexSector currentSector = RootGradient;
            while (true)
            {
                if (currentSector.IsSerial)
                {
                    if (currentSector.IndexElements.Count == 0)
                        throw new Exception("empty sector found");

                    return currentSector.IndexElements[0];
                }
                else
                {
                    currentSector = GetSector(currentSector.IndexElements[0].Reference);
                }
            }
        }
    }
}
