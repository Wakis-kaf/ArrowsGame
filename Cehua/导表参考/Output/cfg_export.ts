 interface sceneItems {
  sceneItemId: number;
  sceneItemCount: number;
}

interface rewards {
  statusRewardId: number;
  statusRewardCount: number;
}

interface status {
  statusId: number;
  rewards: rewards[];
}

interface compositeCfg_item {
  levelId: number;
  levelName: string;
  gameTime: number;
  sceneItems: sceneItems[];
  status: status[];
}

interface compositeCfg {
  "123131": compositeCfg_item;
  "313131": compositeCfg_item;
}

interface commonCfg {
  floatValue: number;
  doubleValue: number;
  booleanValue: boolean;
  intValue: number;
  longIntValue: number;
  stringVallue: string;
  objectValue: any;
  stringArrayValue: string[];
  intArrayValue: number[];
  intArray2DValue: number[][];
}

export interface cfg_export {
  compositeCfg: compositeCfg;
  commonCfg: commonCfg[];
}


/*
exportfileNmae: cfg_export.ts

 
 *****************导出 Excel: 参考表.xlsx,导出 sheet: 复合表: *********** 开始 ***********
 //注释: 关卡id
 levelId,

 //注释: 关卡名称
 levelName,

 gameTime,

 sceneItemId,

 sceneItemCount,

 statusId,

 statusRewardId,

 statusRewardCount,

*****************导出 Excel: 参考表.xlsx,导出 sheet: 复合表: *********** 结束 ***********
 
  


*/

export const config :  cfg_export  = {
	compositeCfg : {
		[123131] : {
			levelId : 123131,levelName : "关卡一",gameTime : 50,sceneItems : [
				{sceneItemId : 100001,sceneItemCount : 10,},
				{sceneItemId : 100002,sceneItemCount : 15,},
				{sceneItemId : 100003,sceneItemCount : 20,},
				{sceneItemId : 100004,sceneItemCount : 25,},
				{sceneItemId : 100005,sceneItemCount : 30,},
				{sceneItemId : 100006,sceneItemCount : 35,},
			
			],
			status : [
				{
					statusId : 101,rewards : [
						{statusRewardId : 20001,statusRewardCount : 100,},
						{statusRewardId : 20002,statusRewardCount : 200,},
					
					],
				
				},
				{
					statusId : 102,rewards : [
						{statusRewardId : 20001,statusRewardCount : 100,},
						{statusRewardId : 20002,statusRewardCount : 200,},
					
					],
				
				},
				{
					statusId : 103,rewards : [
						{statusRewardId : 20001,statusRewardCount : 100,},
						{statusRewardId : 20002,statusRewardCount : 200,},
					
					],
				
				},
				{
					statusId : 104,rewards : [
						{statusRewardId : 20001,statusRewardCount : 100,},
						{statusRewardId : 20002,statusRewardCount : 200,},
					
					],
				
				},
			
			],
		
		},
		[313131] : {
			levelId : 313131,levelName : "关卡2",gameTime : 50,sceneItems : [
				{sceneItemId : 200001,sceneItemCount : 10,},
				{sceneItemId : 200002,sceneItemCount : 15,},
				{sceneItemId : 200003,sceneItemCount : 20,},
				{sceneItemId : 200004,sceneItemCount : 25,},
				{sceneItemId : 200005,sceneItemCount : 30,},
				{sceneItemId : 200006,sceneItemCount : 35,},
				{sceneItemId : 200007,sceneItemCount : 40,},
			
			],
			status : [
				{
					statusId : 201,rewards : [
						{statusRewardId : 20001,statusRewardCount : 300,},
						{statusRewardId : 20002,statusRewardCount : 500,},
					
					],
				
				},
				{
					statusId : 202,rewards : [
						{statusRewardId : 20001,statusRewardCount : 300,},
						{statusRewardId : 20002,statusRewardCount : 500,},
					
					],
				
				},
				{
					statusId : 203,rewards : [
						{statusRewardId : 20001,statusRewardCount : 300,},
						{statusRewardId : 20002,statusRewardCount : 500,},
					
					],
				
				},
				{
					statusId : 204,rewards : [
						{statusRewardId : 20001,statusRewardCount : 300,},
						{statusRewardId : 20002,statusRewardCount : 500,},
					
					],
				
				},
			
			],
		
		},
	
	},
	commonCfg : [
		{floatValue : 1050,doubleValue : 21313.2,booleanValue : true,intValue : 1635,longIntValue : 165156200000,stringVallue : "字符串",objectValue : 123,stringArrayValue : ["你好","123"],intArrayValue : [1561,561],intArray2DValue : [[156,152],[561,658]],},
		{floatValue : 200,doubleValue : 21313.2,booleanValue : true,intValue : 1635,longIntValue : 165156200000,stringVallue : "字符串",objectValue : 123,stringArrayValue : ["你好","123"],intArrayValue : [1561,561],intArray2DValue : [[156,152],[561,658]],},
	
	],

}

