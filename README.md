# SharpBladeGroundStation

某飞控地面站.

VS2015 Update3 + .Net 4.5.2

名字不提供官方翻译,自行领悟.

请勿魔改客户端.

由于开发人员沉迷语法糖无法自拔,不支持 C\# 6 以下版本.

由于开发人员懒的1b不想捕获异常,报异常自行重启解决.

## 配置需求

### 推荐配置

CPU:i9 7900X

GPU:GTX 1080Ti SLI

RAM:32G

分辨率:3840*2160

高速低延迟数传

### 已测试的最低配置

CPU:i5 4210M

GPU:GT840M

RAM:8G

分辨率:1366\*768


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
- [x] 显示
    - [x] 飞行参数显示
    - [x] HUD
    - [x] 多屏支持
    - [x] 地图显示
        - [x] 非法地图
        - [ ] 合法地图
- [ ] 记录回放
    - [x] 记录
    - [x] 回放
    - [x] DVR
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

FlightDisplay(MIT):https://github.com/cfrpg/FlightDisplay

Dynamic Data Display(MIT):https://archive.codeplex.com/?p=dynamicdatadisplay

GMap.NET(MIT):https://archive.codeplex.com/?p=greatmaps

AForge.NET(LGPL,GPL):http://www.aforgenet.com/

## 膜大佬区

测试大佬：CCH

测试大佬：Flanker-A
