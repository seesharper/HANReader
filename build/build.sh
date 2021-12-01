set -e
docker build --pull --no-cache --rm -f Dockerfile -t buildcontainer ..
docker run --rm --env-file build.env buildcontainer build/build.csx

