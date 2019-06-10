tracelog.exe -stop wdm1.sys
tracelog.exe -start wdm1.sys -guid #610D77F9-EA1E-462F-ADDE-6F602C63D056 -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop wdm2.sys
tracelog.exe -start wdm2.sys -guid #913B1202-F711-4974-A7E9-07CDCB9B07D8 -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop cancel.sys
tracelog.exe -start cancel.sys -guid #D3B46EC6-102B-40C6-B3B4-0666C7074391 -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop cancel_a.sys
tracelog.exe -start cancel_a.sys -guid #AAD5F7EE-AC87-4FEE-B907-E093B66B775F -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop filter.sys
tracelog.exe -start filter.sys -guid #DAD9A5A3-EF02-4173-A255-22A1F6C59C19 -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop sm.sys
tracelog.exe -start sm.sys -guid #EF1CF16A-B4B9-485A-A248-A0B507CE7422 -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop kmdfrc.sys
tracelog.exe -start kmdfrc.sys -guid #4FE9BCD9-E5C1-43CB-9D4B-16FFE5BF225E -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop cpplibkrnl.lib
tracelog.exe -start cpplibkrnl.lib -guid #3CB960EF-8F8C-4894-9776-A90212DAEA01 -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop app.exe
tracelog.exe -start app.exe -guid #C66AB2F4-51F9-489E-BABB-E560F9944D47 -level 7 -flag 0xffff -rt -kd
tracelog.exe -stop core.dll
tracelog.exe -start core.dll -guid #827F636D-B08D-4E3B-8258-C86D3CE26EC7 -level 7 -flag 0xffff -rt -kd
