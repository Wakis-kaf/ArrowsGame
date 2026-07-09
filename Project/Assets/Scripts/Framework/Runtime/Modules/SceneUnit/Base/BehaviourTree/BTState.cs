using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSceneUnit.BT
{
    // BTState.cs
    public enum BTState
    {
        Success = 1,
        Fail = 2,
        Running = 3,
        Idle = 4
    }

}
/*
行为树中的节点类型主要有以下几种
组合节点
修饰节点
条件节点
行为节点

组合节点：选择节点、顺序节点、随机选择节点、随机顺序节点、随机权重节点、并行节点、并行执行所有节点、if 判断并行节点、if 判断顺序节点

修饰节点：修饰节点取反、修饰节点重复、修饰节点_返回 Fail、修饰节点_返回 Success、修饰节点_直到 Fail、修饰节点_直到 Success

条件节点

行为节点
*/

/*
下面是各个组合节点的执行逻辑
1.选择节点
选择节点：依次从头顺次遍历执行所有子节点
当前执行节点返回 Success，退出停止，向父节点
返回 Success

当前执行节点返回 Fail，退出当前节点
继续执行下一个节点

当前执行节点返回 Running, 记录当前节点，向父节
点返回 Running，下次执行直接从该节点开始

如果所有节点都返回Fail，执行完所有节点后
向父节点返回 Fail

2.顺序节点
顺序节点：依次执行子节点
当前执行节点返回 Success，就继续执行后续节点

当前执行节点返回 Fail，退出停止，向父节点
返回 Fail，下次执行直接从第一个节点开始

当前执行节点返回 Running, 记录当前节点，向父节
点返回 Running，下次执行直接从该节点开始

如果所有节点都返回 Success，向父节点返回 Success

3.随机选择节点
随机选择节点：(参考选择节点)
每次随机一个未执行的节点，总随机次数为子节点个数

当前执行节点返回 Success，退出停止
向父节点返回 Success

当前执行节点返回 Fail，退出当前节点
继续随机一个未执行的节点开始执行

当前执行节点返回 Running, 记录当前节点
向父节点返回 Running，下次执行直接从该节点开始

如果所有节点都返回Fail，执行完所有节点后
向父节点返回 Fail

4.随机顺序节点
随机顺序节点：(参考顺序节点)
每次随机一个未执行的节点，总随机次数为子节点个数

当前执行节点返回 Success，继续随机一个未执行的节点

当前执行节点返回 Fail，退出停止
向父节点返回 Fail

当前执行节点返回 Running, 记录当前节点
向父节点返回 Running，下次执行直接从该节点开始

如果所有节点都返回 Success
向父节点返回 Success

5.随机权重节点
随机权重节点：(参考随机选择节点)
每次根据节点权重随机一个未执行的节点
总随机次数为子节点个数

当前执行节点返回 Success，退出停止
向父节点返回 Success

当前执行节点返回 Fail，退出当前节点
继续随机一个未执行的节点开始执行

当前执行节点返回 Running, 记录当前节点
向父节点返回 Running
下次执行直接从该节点开始

如果所有节点都返回Fail，执行完所有节点后
向父节点返回 Fail

6.并行节点
并行节点：依次从头顺次遍历执行所有子节点

当前执行节点返回 Fail，退出停止，向父节点
返回 Fail

当前执行节点返回 Success，记录当前节点，继续
执行下一个节点，记录所有返回 Success 的节点

当前执行节点返回 Running, 记录当前节点，继续
执行下一个节点，记录所有返回 Running 的节点

如果没有节点返回 Fail
如果所有节点都返回 Success 向父节点返回 Success
否则向父节点返回 Running

7.并行执行所有节点
并行执行所有节点：依次从头顺次遍历执行所有子节点

当前执行节点返回 Success、 Fail、Running 都继续
执行下一个节点，分别记录返回三种结果的节点个数

执行完所有节点后
如果所有节点都返回 Success 向父节点返回 Success
如果所有节点都返回 Fail 向父节点返回 Fail
否则一定有节点返回了Running 向父节点返回 Running

8.if 判断并行节点
if判断并行节点：
只能有 二或者三个子节点
第一个为判断节点只能返回Success、Fail

因为是并行节点，每次执行都会先执行第一个节点
根据第一个节点返回结果选择执行第二个、第三个节点

如果上次执行的是第二、三个节点中的某一个
当前要执行的节点跟上次相同，则会直接执行 Execute
如果当前要执行的节点跟上次不同，则会执行上次节点
OnExit , 新节点则走 OnEnter、Execute

9.if 判断顺序节点
if判断顺序节点：
只能有 二或者三个子节点
第一个为判断节点只能返回Success、Fail

因为是顺序节点，每次执行时
如果当前有正在执行的第二、第三个节点则
直接执行它的 Execute

如果没有，则执行第一个节点，根据第一个节点返回
结果 Success、Fail，选择执行第二、第三个节点

10.修饰节点取反
取反修饰节点 Inverter 对子节点执行结果取反

11.修饰节点重复
修饰节点_重复:
开始执行该节点时，将记录次数清零
顺序执行所有子节点(记为 1 次)，不关心节点返回结果
如果 执行次数 < 配置执行次数 向父节点返回 Running
如果 执行次数 >= 配置执行次数 向父节点返回 Success

12.修饰节点_返回 Fail
修饰_返回Fail：
执行节点，无论节点返回 Success、Fail、Running
执行结束后永远向父节点返回 Fail

13.修饰节点_返回 Success
修饰_返回Success：
执行节点，无论节点返回 Success、Fail、Running
执行结束后永远向父节点返回 Success

14.修饰节点_直到 Fail
修饰_直到Fail：
执行节点
如果节点返回结果不是 Fail
向父节点返回 Running

直到节点返回 Fail，向父节点返回 Success

15.修饰节点_直到 Success
修饰_直到Success：
执行节点
如果节点返回结果不是 Success
向父节点返回 Running

直到节点返回 Success，向父节点返回 Success
*/