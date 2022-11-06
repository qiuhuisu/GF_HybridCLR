# GF_HybridCLR

#### 介绍
GameFramework 接入HybridCLR

#### 软件架构
软件架构说明


#### 安装教程

1.  Unity菜单栏 HybridCLR->Installer，点击安装
2.  先Build一次工程以生成dll给HybridCLR使用，Build时会自动执行菜单的HybridCLR->Generate->All功能生成热更桥接函数
3.  HybridCLR->CompileDllAndCopy生成热更dll并自动复制到项目热更dll目录 Assets/AAAGame/HotfixDlls
4.  打AB包，Launch场景 GameFramework->Builtin->Resource节点，Inspector面板设置ResourceMode有Package(单机)/Updatable(热更新)/UpdatableWhilePlaying(边玩边更)；
单机：ResourceMode选择Package，GameFramework->ResourceBuilder界面只勾选Output Package Path， 点击Start Build打包并自动复制AB包到StreamingAssets
热更：ResourceMode选择Updatable，GameFramework->ResourceBuilder界面只勾选Output Full Path， 点击Start Build打包成功后在Output Full Path对应目录中会生成对应平台的AB包，其中version.json文件的UpdatePrefixUri字段为热更下载地址；如Android，将整个Android文件夹上传到UpdatePrefixUri配置的文件服务器即可；
5.  Build出包

#### 使用说明

1.  开发目录为Assets/AAAGame, Script为热更新脚本目录，ScriptBuiltin为内置程序集；
2.  Assets/AAAGame/Scene/Launch场景为游戏的启动场景
3.  xxxx



