# GF_HybridCLR

#### 介绍
GameFramework 接入HybridCLR

#### 软件架构
软件架构说明
### GF_HybridCLR功能说明：
1. 简化和扩展GameFramework接口，并支持GF所有异步加载的方法通过Task伪同步加载。
2. 增加各种编辑器工具，简化工作流。如:生成数据表/配置表, 自动解决AB包重复依赖, 代码裁剪link.xml配置工具,语言国际化生成工具，一键切换单机/全热更/增量热更，一键打包/打热更工具。
3. 支持A/B Test; 使用GF.Setting.SetABTestGroup("GroupName")对用户分配测试组，不同测试组会读取对应组的配置表/数据表。

#### 安装教程

1.  首次使用需安装HybridCLR环境，点击Unity顶部菜单栏 【HybridCLR->Installer】安装HybridCLR环境;
2.  Unity工具栏 【Build App/Hotfix】按钮打开一键打包界面; 首次打热更App需点击【Build App】按钮右侧的下拉菜单，点击【Full Build】构建；
3.  Build App出包，Build Resource打热更;

#### 使用说明

1.  开发目录为Assets/AAAGame, Script为热更新脚本目录，ScriptBuiltin为内置程序集；
2.  Assets/AAAGame/Scene/Launch场景为游戏的启动场景


#### 工作流
前言:
首先要理清游戏核心元素，游戏中可见的元素一共就两种，UI界面和GameObject物体(模型、Sprite、粒子等)

可见元素：GF简单得分为两种，即UIForm和Entity(UI和实体)，分别通过UIComponent和EntityComponent管理。（实体Entity可以理解为任何GameObject）
不可见元素: 即游戏的业务逻辑，GF提供了流程(Procedure)，通过有限状态机来管理游戏流程，使游戏逻辑结构清晰。

此框架对GF又进一步封装,使用更加简单便捷:

UIForm: 使用 GF.UI.OpenUIForm() 打开UI, GF.UI.CloseUIForm()关闭UI； 开发者只管打开/关闭，GF内部会自动通过对象池管理复用或销毁释放
Entity：使用 GF.Entity.ShowEntity()显示实体， GF.Entity.HideEntity()隐藏实体； 同样,开发者只需管显示/隐藏，GF会自动对象池复用或销毁释放。
Procedure：它拥有类似MonoBehaviour的生命周期，如OnEnter(进入流程)、OnUpdate(每帧刷新)、OnLeave(离开流程)，在流程内调用ChangeState()即可切换到其它流程

GF_HybridCLR通过Procedure(流程)来走游戏逻辑，游戏入口流程为LaunchProcedure
通过流程可以清晰地管理游戏逻辑，例如：本框架的基本流程为：

1. LaunchProcedure(启动流程,主要处理初始化游戏设置,如语言，音效，震动等) => 完成后切换至CheckAndUpdateProcedure
2. CheckAndUpdateProcedure(此流程处理单机/热更模式的资源初始化逻辑，处理App升级和资源更新以及初始化资源，只需要打AB资源时配置热更地址等即可实现完整热更) => 资源准备就绪，切换至LoadHotfixDllProcedure
3. LoadHotfixDllProcedure(主要为HybridCLR做准备，如加载热更dll，AOT泛型补充。 反射调用热更类HotfixEntry.StartHotfixLogic创建新的流程状态机) => 开始进入热更代码的Procedure

以上为Builtin(内置)代码，逻辑上比较靠前，不可以热更新。至此之后，逻辑被热更新代码接管，进入PreloadProcedure

4. PreloadProcedure(预加载流程，会添加GF框架扩展功能、设置AB测试组， 加载数据表/配置表/多语言) => 切换至ChangeSceneProcedure
5. ChangeSceneProcedure(此流程专门用于切换场景，不同的场景对应切换至不同的流程) => 切换至Game场景，同时流程切换为MenuProcedure
6. MenuProcedure(即游戏的主菜单流程, 流程OnEnter时显示主菜单UI，通常有开始游戏入口、商店入口、设置入口等，流程OnLeave时关闭主菜单等) => 若玩家点击开始，切换至GameProcedure
7. GameProcedure(游戏玩法流程，例如: 加载游戏关卡地图，创建玩家、敌人等) => 游戏胜利/失败，切换到GameOverProcedure，显示游戏胜利/失败界面，进行游戏结算等
8. GameOverProcedure(游戏结束流程, 通常会显示游戏胜利/失败界面, 进行游戏奖励结算, 并显示下一关、重玩按钮或返回主页等按钮) => 如点击下一关, 把关卡Id改为下一关然后切换到GameProcedure就开始下一关游戏了。点击返回主页按钮切换到MenuProcedure就回到了游戏主菜单。

使用有限状态机Procedure管理游戏逻辑，结构清晰，逻辑简单。无论是维护还是debug都非常容易切入，使用Procedure可以规范开发