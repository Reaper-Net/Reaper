FROM ubuntu:latest

ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get -yqq update>/dev/null && \
    apt-get -yqq install >/dev/null curl wrk