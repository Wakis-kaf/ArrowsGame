namespace Game.Modules.GModuleInventory
{
    public struct Item
    {
        public const int ItemStatus_UnLock = 0;
        public const int ItemStatus_Got = 1;
        public int count;
        public int status;
        public ItemUniqueGroup uniqueGroup;
        public bool enableStack;
        public bool isUnique;
        public string iconSpritePath;
        public string iconTexPath;
        public int itemId;
        public string itemName;
        public int maxHoldCount;
        public int maxPerStackCount;
        public int maxStackCount; // 最大对数 -1为无穷
        public bool showRed;
        public bool showDouble;
        public bool showFlow;
        // 每堆最大数量，-1为无穷 最大持有数量，-1为无穷
    }
}