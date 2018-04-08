# SharpBladeGroundStation

某飞控地面站.

VS2015 Update3 + .Net 4.5.2

名字不提供官方翻译,自行领悟.

请勿魔改客户端.

懒得捕获异常,报异常自行重启解决.

## 配置需求

### 推荐配置

CPU:i9 7900X

GPU:GTX 1080Ti SLI

RAM:32G

分辨率:3840*2160

### 已测试的最低配置

CPU:i5 6200U

GPU:HD520

RAM:8G

分辨率:2736\*1824 200% (1368\*912)


## 预定功能

### 第一期
- [x] 通信
    - [x] 串口扫描
    - [x] 串口收发
    - [x] 拆包封包
        - [x] ANOLink
        - [x] MAVLink 1.0
        - [ ] MAVLink 2.0
        - [ ] SBLink?
- [ ] 无人机控制
    - [ ] 航线规划
    - [ ] 协议收发
    - [x] 飞控指令
- [ ] 显示
    - [x] 飞行参数显示
    - [ ] HUD
    - [x] 多屏支持
    - [x] 地图显示
        - [x] 非法地图
        - [ ] 合法地图
- [ ] 记录回放
    - [ ] 记录
    - [ ] 回放
    - [ ] DVR
- [ ] 系统
    - [ ] 固件更新
    - [x] 系统设置
    - [x] 配置加载

### 第二期
- [ ] 这辣鸡地面站还有第二期？

### 鱼
- [x] 鱼
- [ ] 好大的鱼
- [ ] 虎纹鲨鱼
- [ ] 拿铁犁捞大鱼

## 预定文档

- [x] SGS
- [ ] SDD
- [ ] SUM

## 引用库

FlightDisplay:https://github.com/cfrpg/FlightDisplay

WPF MediaKit:https://github.com/Sascha-L/WPF-MediaKit

Dynamic Data Display:https://archive.codeplex.com/?p=dynamicdatadisplay

DirectShow.Net:http://directshownet.sourceforge.net/

GMap.NET:https://archive.codeplex.com/?p=greatmaps
