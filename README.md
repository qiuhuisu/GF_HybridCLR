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
