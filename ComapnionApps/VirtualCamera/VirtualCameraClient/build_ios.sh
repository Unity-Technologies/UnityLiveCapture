#!/bin/bash

set -e
echo "Bootstrapping your computer..."

export LC_ALL=en_US.UTF-8
export LANG=en_US.UTF-8

unameOut="$(uname -s)"
case "${unameOut}" in
    Linux*)     machine=Linux;;
    Darwin*)    machine=Mac;;
    *)          machine="UNKNOWN:${unameOut}"
esac
echo "Running on: ${machine}"

# Check for Homebrew, install if we don't have it
if [ "${machine}" == "Mac" ]; then
  if test ! $(which brew); then
    echo "Installing homebrew..."
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install.sh)"
    echo "Ensuring permissions are correct on /usr/local/.. folders. Need sudo"
    sudo chown -R $(whoami) $(brew --prefix)/*
  else
    echo "Homebrew already installed..."
  fi

  if test ! "$(which ruby)"; then
    echo "Install ruby..."
    brew install ruby
  else
    echo "Ruby already installed..."
  fi
  
  if test ! $(which fastlane); then
    echo "Install fastlane tools..."
    brew install fastlane
  else
    echo "Fastlane already installed..."
  fi
  if test ! $(which ios-deploy); then
    echo "Install ios-deploy tools..."
    brew install ios-deploy
  else
    echo "ios-deploy already installed..."
  fi
#  if test ! $(which dotenv); then
#    echo "Install dotenv for fastlane..."
#    sudo gem install dotenv
#  else
#    echo "dotenv already installed..."
#  fi
#  if test ! $(which bundler); then
#    echo "Install bundler for fastlane..."
#    sudo gem install bundler
#    sudo bundle update --bundler
#  else
#    echo "bundler gem already installed... Will update"
#    sudo bundle update --bundler
#  fi
fi


# Time to build
fastlane local_build

## KEEPING below for future reference if we want to add switch flags
# Set vars from command line options to push to fastlane
#RELEASE_TYPE=$1
#usage="$(basename "$0") [-h] [-p n] -- program to calculate the answer to life, the universe and everything
#where:
#    -h  show this help text
#    -p  profile to build (default: development) options: [development, adhoc]"
#
#POSITIONAL=()
#while [[ $# -gt 0 ]]
#do
#key="$1"
#
#case $key in
#    -p|--profile)
#    PROFILE="$2"
#    shift # past argument
#    shift # past value
#    fastlane local_build release_type:"$PROFILE"
#    ;;
#    -s|--searchpath)
#    SEARCHPATH="$2"
#    shift # past argument
#    shift # past value
#    ;;
#    -l|--lib)
#    LIBPATH="$2"
#    shift # past argument
#    shift # past value
#    ;;
#    -h|--help)
#    echo "$usage"
#    ;;
#    *)    # unknown option
#    echo "$usage"
#    POSITIONAL+=("$1") # save it in an array for later
#    shift # past argument
#    ;;
#esac
#done
#set -- "${POSITIONAL[@]}" # restore positional parameters
#
#echo "PROFILE  = ${PROFILE}"
