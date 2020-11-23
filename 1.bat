

	git add .
	git clean
	git commit -m "foo"
	git push  -u origin master 
	git pull
	docker build  . -t test004
	rem dotnet build
	rem rem 是註解
	rem rem