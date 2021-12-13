using System;

namespace udSDK
{
    /// <summary>
    /// The API versions associated with udSDK.
    /// </summary>
    public enum APIVersion
    {
        Offline,
        udServer,
        udCloud
    };

    /// <summary>
    /// The type of udCloud Node.
    /// </summary>
    public enum UDCloudNodeType
    {
        udCloud_User,
        udCloud_Workspace,
        udCloud_Project,
        udCloud_Scene,
        udCloud_Folder,
        udCloud_File
    }

    public class UDCloudNode
    {
        /// <summary>
        /// The UUID of this node.
        /// </summary>
        public string uuid;

        /// <summary>
        /// The name of this node.
        /// </summary>
        public string name;

        /// <summary>
        /// The type of node this is. Automagically generated based on the depth of this node
        /// </summary>
        ///  <remarks> Though subject to change to handle folder/file depth discrepancy.</remarks>
        public UDCloudNodeType type { get { return (UDCloudNodeType)depth; } }

        /// <summary>
        /// The children of this node. NULL if no children exist.
        /// </summary>
        public UDCloudNode[] children;

        /// <summary>
        /// The parent of this node. NULL if no parent exists.
        /// </summary>
        public UDCloudNode parent;

        /// <summary>
        /// If this node has children.
        /// </summary>
        public bool hasChildren { get { return children == null ? true : false; } }

        /// <summary>
        /// The array of siblings associated with this node.
        /// </summary>
        public UDCloudNode[] siblings { get { return parent.children; } }

        /// <summary>
        /// The index of this node with respect to its parent. Returns 0 if no parent exists.
        /// </summary>
        public int index
        {
            get
            {
                int i = 0;

                if (parent != null)
                    for (i = 0; i < siblings.Length; i++)
                        if (siblings[i].uuid == uuid)
                            break;

                return i;
            }
        }

        /// <summary>
        /// The size of the tree extending from this node.
        /// </summary>
        public int treeSize
        {
            get
            {
                int count = 1;

                if (children == null)
                    return count;

                for (int i = 0; i < children.Length; i++)
                    count += children[i].treeSize;

                return count;
            }
        }

        /// <summary>
        /// The index of the node with respect to the root node.
        /// <para>(e.g.)</para>
        /// <code>
        /// d=0               [0]        
        /// -               /  |  \      
        /// d=1          [6]  [5]  [1]    
        /// -           /   \     /   \
        /// d=2       [10]  [7] [4]   [2] 
        /// -           |   / \        |              
        /// d=3       [11][9] [8]     [3] 
        /// </code>
        /// </summary>
        public int rootIndex
        {
            get
            {
                int pos = 0;

                UDCloudNode node = this;

                while (node.uuid != root.uuid)
                {
                    if (node.index > 0)
                        for (int i = 0; i < node.index; i++)
                            pos += node.siblings[i].treeSize;

                    pos++;

                    node = node.parent;
                }

                return pos;
            }
        }

        /// <summary>
        /// The depth level of the node.
        /// </summary>
        public int depth
        {
            get
            {
                int count = 0;

                if (parent == null)
                    return count;

                UDCloudNode node = parent;

                while (node != null)
                {
                    count++;
                    node = node.parent;
                }

                return count;
            }
        }

        /// <summary>
        /// The root node of this node.
        /// </summary>
        public UDCloudNode root
        {
            get
            {
                if (parent == null)
                    return this;

                UDCloudNode node = parent;

                while (node.parent != null)
                    node = node.parent;

                return node;
            }
        }

        /// <summary>
        /// Gets the index of this node from a specified <paramref name="root"/>. 
        /// Does not need to be the real <paramref name="root"/> of this node.
        /// If the specified <paramref name="root"/> is shallower than this node, returns -depth.
        /// </summary>
        /// <param name="root"> The node that the index returned is with respect to. </param>
        /// <returns>The index of this node with respect to the specified <paramref name="root"/> </returns>
        public int GetRootIndexFrom(UDCloudNode root)
        {
            int pos = 0;

            UDCloudNode self = this;

            if (root.depth >= self.depth) //if from node is shallower than node to check, we add -1 for every parent.
                return pos - (root.depth - self.depth);

            while (self.uuid != root.uuid)
            {
                pos++;

                if (self.index > 0)
                    for (int i = 0; i < self.index; i++)
                        pos += self.siblings[i].treeSize + 1;

                self = self.parent;
            }

            return pos;
        }


    }

    /// <summary>
    /// A Unity serializable struct which the JSONUtility can map the <i>contents</i> of the list.json files from the udCloud API.
    /// </summary>
    [Serializable]
    public struct UDCloudJSON
    {
        public string id;
        public string name;
    }

    /// <summary>
    /// A Unity serializable class that contains arrays of udCloudJson, such that the JSONUtility can map the list.json files from the udCloud API.
    /// </summary>
    [Serializable]
    public class UDCloudQuery
    {
        public UDCloudJSON[] organisations;
        public UDCloudJSON[] projects;
        public UDCloudJSON[] scenes;
    }
}