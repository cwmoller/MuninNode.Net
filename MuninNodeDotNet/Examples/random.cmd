@echo off

IF [%1] == [] (
	echo rnd.value 42
	echo .
	exit 0
)

IF [%1] == [config] (
	echo graph_title Random number test
	echo graph_category test
	echo graph_args --upper-limit 100 -l 0
	echo rnd.label Random number
	echo .
	exit 0
)

echo .
