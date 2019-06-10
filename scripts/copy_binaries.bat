@echo off
xcopy %1 "d:\SharedFolder" /s /e /y
echo copied %1 to d:\SharedFolder