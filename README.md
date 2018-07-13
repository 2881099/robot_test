# robot_test
简易任务调度Demo .net core console NJob Demo

```txt
; 和 # 匀为行注释
;SEC：				按秒触发
;MIN：				按分触发
;HOUR：				按时触发
;DAY：				按天触发
;RunOnDay：			每天 什么时间 触发
;RunOnWeek：			星期几 什么时间 触发
;RunOnMonth：			每月 第几天 什么时间 触发

;Name1		SEC		2			/schedule/test002.aspx
;Name2		MIN		2			/schedule/test002.aspx
;Name3		HOUR		1			/schedule/test002.aspx
;Name4		DAY		2			/schedule/test002.aspx

;Name5		RunOnDay	15:55:59		/schedule/test002.aspx
;每天15点55分59秒

;Name6		RunOnWeek	1:15:55:59		/schedule/test002.aspx
;每星期一15点55分59秒

;Name7		RunOnMonth	1:15:55:59		/schedule/test002.aspx
;每月1号15点55分59秒
```