 export declare type TS_Ｄ_GOODSITEMSCONFIG = {
goodsItems :TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMS, 
avatarGoodsItems :TS_Ｄ_GOODSITEMSCONFIG_AVATARGOODSITEMS, 
goodsItemType :TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMTYPE[], 
goodsComposeRule :TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE, 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMS = {
1010010001 :TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMS_1010010001, 
1020040001 :TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMS_1010010001, 
1020020001 :TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMS_1010010001, 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMS_1010010001 = {
itemId :number, 
firstType :string, 
secondType :string, 
itemName :string, 
bagShow :boolean, 
useCount :number, 
useCostItemId :number, 
useCostItemCount :number, 
canDestroy :boolean, 
useSceneType :number, 
oneStackUpLimit :number, 
stackUpLimit :number, 
icon :string, 
useWayDescription :string, 
getWayDescription :string, 
spaceNeed :number, 
stackNeedSpace :boolean, 
weight :number, 
stackNeedWeight :boolean, 
assetKey :string, 
buffId :string[], 
interactiveName :string, 
interactiveEvent :string, 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_AVATARGOODSITEMS = {
1060010001 :TS_Ｄ_GOODSITEMSCONFIG_AVATARGOODSITEMS_1060010001, 
1060020001 :TS_Ｄ_GOODSITEMSCONFIG_AVATARGOODSITEMS_1060010001, 
1060030001 :TS_Ｄ_GOODSITEMSCONFIG_AVATARGOODSITEMS_1060010001, 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_AVATARGOODSITEMS_1060010001 = {
itemId :number, 
firstType :string, 
secondType :string, 
itemName :string, 
bagShow :boolean, 
useCount :number, 
useCostItemId :number, 
useCostItemCount :number, 
canDestroy :boolean, 
useSceneType :number, 
oneStackUpLimit :number, 
stackUpLimit :number, 
icon :string, 
useWayDescription :string, 
getWayDescription :string, 
spaceNeed :number, 
stackNeedSpace :boolean, 
weight :number, 
stackNeedWeight :boolean, 
assetKey :string[], 
mountBoneName :string[], 
buffIds :string[][], 

}

export declare type TS_Ｄ_GOODSITEMSCONFIG_GOODSITEMTYPE = {
firstId :string, 
firstTypeName :string, 
secondId :string, 
secondTypeName :string, 
commit :string, 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE = {
5100101 :TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE_5100101, 
5100201 :TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE_5100101, 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE_5100101 = {
multiRowData :(TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE_5100101_MULTIROWDATA|TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE_5100101_MULTIROWDATA_2)[], 
skillId :number, 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE_5100101_MULTIROWDATA = {
skillLevel :number, 
hp :number, 
attack :number, 
defend :number, 
baoji :number, 
buffIds :string[][], 

}
export declare type TS_Ｄ_GOODSITEMSCONFIG_GOODSCOMPOSERULE_5100101_MULTIROWDATA_2 = {
skillLevel :number, 
hp :number, 
attack :number, 
defend :number, 
baoji :number, 
buffIds :any, 

}


/*
exportfileNmae: GoodsItemsConfig.ts

 
 *****************导出 Excel: 物品配置.xlsx,导出 sheet: 物品配置: *********** 开始 ***********
 //注释: 第1位固定为1; 23位为一级分类; 456位为二级分类; 6789位为序号; （按照ID大小; 顺序向下加）
 //注释: ID
 itemId,

 //注释: 物品一级分类具体，详见道具类型规则说明
 //注释: 一级分类
 firstType,

 //注释: 物品二级分类具体，详见道具类型规则说明
 //注释: 二级分类
 secondType,

 //注释: 
 //注释: 物品名
 itemName,

 //注释: 背包可见，0为不可见,1为可见
 //注释: 背包可见
 bagShow,

 //注释: 使用时，消耗该物品的次数
 //注释: 使用次数
 useCount,

 //注释: 
 //注释: 使用物品消耗ID
 useCostItemId,

 //注释: 
 //注释: 使用道具消耗次数
 useCostItemCount,

 //注释: 1 可销毁，0 不可销毁
 //注释: 是否可销毁
 canDestroy,

 //注释: 0 - 通用 ; 1 - 生存; 2 - 创造
 //注释: 使用场景
 useSceneType,

 //注释: 单堆一次堆叠上限; -1 为无穷
 //注释: 堆叠上限
 oneStackUpLimit,

 //注释: ‘-1 为无穷
 //注释: 堆上限
 stackUpLimit,

 //注释: icon
 //注释: 图标
 icon,

 //注释: 
 //注释: 使用方式描述
 useWayDescription,

 //注释: 
 //注释: 获取方式描述
 getWayDescription,

 //注释: 
 //注释: 存放需要消耗的空间
 spaceNeed,

 //注释: 
 //注释: 堆叠是否消耗空间
 stackNeedSpace,

 //注释: 
 //注释: 占据背包重量
 weight,

 //注释: 
 //注释: 堆叠是否增加背包重量
 stackNeedWeight,

 //注释: 
 //注释: 资源键值
 assetKey,

 //注释: Buff 效果Id,详情参见Buff表
 //注释: 特殊效果BuffId
 buffId,

 //注释: 
 //注释: 交互输入名
 interactiveName,

 //注释: 
 //注释: 交互事件
 interactiveEvent,

*****************导出 Excel: 物品配置.xlsx,导出 sheet: 物品配置: *********** 结束 ***********
*****************导出 Excel: 物品配置.xlsx,导出 sheet: 服装物品配置: *********** 开始 ***********
 //注释: 第1位固定为1; 23位为一级分类; 456位为二级分类; 6789位为序号; （按照ID大小; 顺序向下加）
 //注释: ID
 itemId,

 //注释: 物品一级分类具体，详见道具类型规则说明
 //注释: 一级分类
 firstType,

 //注释: 物品二级分类具体，详见道具类型规则说明
 //注释: 二级分类
 secondType,

 //注释: 
 //注释: 物品名
 itemName,

 //注释: 背包可见，0为不可见,1为可见
 //注释: 背包可见
 bagShow,

 //注释: 使用时，消耗该物品的次数
 //注释: 使用次数
 useCount,

 //注释: 
 //注释: 使用物品消耗ID
 useCostItemId,

 //注释: 
 //注释: 使用道具消耗次数
 useCostItemCount,

 //注释: 1 可销毁，0 不可销毁
 //注释: 是否可销毁
 canDestroy,

 //注释: 0 - 通用 ; 1 - 生存; 2 - 创造
 //注释: 使用场景
 useSceneType,

 //注释: 单堆一次堆叠上限; -1 为无穷
 //注释: 堆叠上限
 oneStackUpLimit,

 //注释: ‘-1 为无穷
 //注释: 堆上限
 stackUpLimit,

 //注释: icon
 //注释: 图标
 icon,

 //注释: 
 //注释: 使用方式描述
 useWayDescription,

 //注释: 
 //注释: 获取方式描述
 getWayDescription,

 //注释: 
 //注释: 存放需要消耗的空间
 spaceNeed,

 //注释: 
 //注释: 堆叠是否消耗空间
 stackNeedSpace,

 //注释: 
 //注释: 占据背包重量
 weight,

 //注释: 
 //注释: 堆叠是否增加背包重量
 stackNeedWeight,

 //注释: 
 //注释: 资源键值
 assetKey,

 //注释: 
 //注释: 挂载的骨骼节点
 mountBoneName,

 //注释: Buff 效果Id,详情参见Buff表 multiColArray#1#buffIds#int
 //注释: 特殊效果BuffId
 buffId,

 //注释: 
 //注释: 
 ,

*****************导出 Excel: 物品配置.xlsx,导出 sheet: 服装物品配置: *********** 结束 ***********
 
  


*/

let config :  TS_Ｄ_GOODSITEMSCONFIG  = {
	goodsItems : {
		[1010010001] : {itemId : 1010010001,firstType : "01",secondType : "001",itemName : "野果",bagShow : true,useCount : 1,useCostItemId : 1010010001,useCostItemCount : 1,canDestroy : false,useSceneType : 0,oneStackUpLimit : -1,stackUpLimit : 1,icon : "prop/berries.png",useWayDescription : "食用后会增加5点体力",getWayDescription : "",spaceNeed : 1,stackNeedSpace : false,weight : 1,stackNeedWeight : false,assetKey : "",buffId : ["2001"],interactiveName : "",interactiveEvent : "",},
		[1020040001] : {itemId : 1020040001,firstType : "01",secondType : "004",itemName : "牛肉",bagShow : true,useCount : 1,useCostItemId : 1020040001,useCostItemCount : 1,canDestroy : false,useSceneType : 0,oneStackUpLimit : -1,stackUpLimit : 1,icon : "prop/meat_fresh.png",useWayDescription : "食用后会增加10点体力",getWayDescription : "",spaceNeed : 1,stackNeedSpace : false,weight : 1,stackNeedWeight : false,assetKey : "",buffId : ["2006"],interactiveName : "",interactiveEvent : "",},
		[1020020001] : {itemId : 1020020001,firstType : "02",secondType : "002",itemName : "金币",bagShow : true,useCount : 1,useCostItemId : 1020020001,useCostItemCount : 1,canDestroy : false,useSceneType : 0,oneStackUpLimit : -1,stackUpLimit : 1,icon : "",useWayDescription : "交易使用",getWayDescription : "",spaceNeed : 1,stackNeedSpace : false,weight : 1,stackNeedWeight : false,assetKey : "",buffId : [],interactiveName : "",interactiveEvent : "",},
	
	},
	avatarGoodsItems : {
		[1060010001] : {
			itemId : 1060010001,firstType : "06",secondType : "001",itemName : "头巾1",bagShow : true,useCount : 1,useCostItemId : 1060010001,useCostItemCount : 1,canDestroy : false,useSceneType : 0,oneStackUpLimit : 1,stackUpLimit : 1,icon : "",useWayDescription : "",getWayDescription : "",spaceNeed : 1,stackNeedSpace : false,weight : 1,stackNeedWeight : false,assetKey : ["Chr_HeadCoverings_Base_Hair_01"],mountBoneName : ["Spine_03"],buffIds : [
				["2002","2004",],
			
			],
		
		},
		[1060020001] : {
			itemId : 1060020001,firstType : "06",secondType : "002",itemName : "头发1",bagShow : true,useCount : 1,useCostItemId : 1060020001,useCostItemCount : 1,canDestroy : false,useSceneType : 0,oneStackUpLimit : 1,stackUpLimit : 1,icon : "",useWayDescription : "",getWayDescription : "",spaceNeed : 1,stackNeedSpace : false,weight : 1,stackNeedWeight : false,assetKey : ["Chr_Hair_01"],mountBoneName : ["Spine_03"],buffIds : [
				["2002","2004",],
			
			],
		
		},
		[1060030001] : {
			itemId : 1060030001,firstType : "06",secondType : "003",itemName : "头盔1",bagShow : true,useCount : 1,useCostItemId : 1060030001,useCostItemCount : 1,canDestroy : false,useSceneType : 0,oneStackUpLimit : 1,stackUpLimit : 1,icon : "",useWayDescription : "",getWayDescription : "",spaceNeed : 1,stackNeedSpace : false,weight : 1,stackNeedWeight : false,assetKey : ["Chr_HelmetAttachment_01"],mountBoneName : ["Spine_03"],buffIds : [
				["2003","2005",],
			
			],
		
		},
	
	},
	goodsItemType : [
		{firstId : "01",firstTypeName : "食物",secondId : "001",secondTypeName : "可食用食物",commit : "",},
		{firstId : "01",firstTypeName : "食物",secondId : "002",secondTypeName : "不可食用食物",commit : "",},
	
	],
	goodsComposeRule : {
		[5100101] : {
			multiRowData : [
				{
					skillLevel : 1,hp : 150,attack : 0,defend : 0,baoji : 0,buffIds : [
						["2002","2004",],
					
					],
				
				},
				{
					skillLevel : 2,hp : 300,attack : 0,defend : 0,baoji : 0,buffIds : [
						["2002","2004",],
					
					],
				
				},
				{
					skillLevel : 3,hp : 450,attack : 0,defend : 0,baoji : 0,buffIds : [
						["2003","2005",],
					
					],
				
				},
				{
					skillLevel : 4,hp : 600,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 5,hp : 750,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 6,hp : 900,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 7,hp : 1050,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 8,hp : 1200,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 9,hp : 1350,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 10,hp : 1500,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 11,hp : 1650,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 12,hp : 1800,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 13,hp : 1950,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 14,hp : 2100,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 15,hp : 2250,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 16,hp : 2400,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 17,hp : 2550,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 18,hp : 2700,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 19,hp : 2850,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 20,hp : 3000,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 21,hp : 3150,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 22,hp : 3300,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 23,hp : 3450,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 24,hp : 3600,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 25,hp : 3750,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 26,hp : 3900,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 27,hp : 4050,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 28,hp : 4200,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 29,hp : 4350,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 30,hp : 4500,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 31,hp : 4650,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 32,hp : 4800,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 33,hp : 4950,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 34,hp : 5100,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 35,hp : 5250,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 36,hp : 5400,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 37,hp : 5550,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 38,hp : 5700,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 39,hp : 5850,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 40,hp : 6000,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 41,hp : 6150,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 42,hp : 6300,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 43,hp : 6450,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 44,hp : 6600,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 45,hp : 6750,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 46,hp : 6900,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 47,hp : 7050,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 48,hp : 7200,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 49,hp : 7350,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 50,hp : 7500,attack : 0,defend : 0,baoji : 0,buffIds : {},
				
				},
			
			],
			skillId : 0,
		},
		[5100201] : {
			multiRowData : [
				{
					skillLevel : 1,hp : 0,attack : 100,defend : 0,baoji : 0,buffIds : [
						["","",],
					
					],
				
				},
				{
					skillLevel : 2,hp : 0,attack : 200,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 3,hp : 0,attack : 300,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 4,hp : 0,attack : 400,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 5,hp : 0,attack : 500,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 6,hp : 0,attack : 600,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 7,hp : 0,attack : 700,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 8,hp : 0,attack : 800,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 9,hp : 0,attack : 900,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 10,hp : 0,attack : 1000,defend : 0,baoji : 0,buffIds : {},
				
				},
				{
					skillLevel : 11,hp : 0,attack : 1100,defend : 0,baoji : 0,buffIds : {},
				
				},
			
			],
			skillId : 0,
		},
	
	},

}
