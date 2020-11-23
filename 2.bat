
    powershell -command "docker rm $(docker stop $(docker ps -a -q --filter ancestor=test004 --format=\"{{.ID}}\"))"
    git pull
    docker build  . -t test004
    docker run --name checkAlive -p 5567:8080 test004
pause