# Contribution and maintenance

First of all, thank you for considering being a contributor to this repository!

This page includes a set of guidelines for contributors as well as information and instructions about the Live Capture team maintenance process.

## Contribution agreement

**All Contributions to this repository are subject to the [Unity Contribution Agreement (UCA)](https://unity3d.com/legal/licenses/Unity_Contribution_Agreement).**

By making a pull request, you are confirming agreement to the terms and conditions of the UCA, including that your Contributions are your original creation and that you have complete right and authority to make your Contributions.

## How to contribute

### Report bugs

The best way to report bugs is to use the [Unity reporting tool](https://unity.com/releases/editor/qa/bug-reporting).

### Suggest enhancements

While we're not currently accepting feature requests, we're open to reviewing incoming pull requests, including those 
that introduce new features. Your contributions will be appreciated.

### Make pull requests

Once you have a change ready, make a pull request in GitHub.   

The pull request description should include a summary of the change and its context, a description of the fixed issues, and any other relevant information that would help the review.
If you're fixing a bug, please include steps to reproduce, environment information, and screenshots/screencasts as relevant.

Adding an entry in the CHANGELOG.md is not required, but is appreciated.

Discussions on pull requests should be limited to the review of the content (code or documentation) changed via the pull request itself.

### Get help

If you need help, ask your questions and engage with the community on the [Unity Forum](https://forum.unity.com/forums/unity-live-capture.673/).

## Branching workflow

The `main` branch is where the development changes for the next release are. This branch
also known as the “integration branch”.   
Branch from `main` to implement any enhancements or documentation updates, etc. 

If you fix a bug, branch off the tagged version with the bug and then make sure to port the fix to `main`.

We also use publish branches for each major and minor.   
Example:
`live-capture/4.0/release`

## Changelog and versioning

All notable changes to this package should be documented in the [Changelog](Packages/com.unity.live-capture/CHANGELOG.md) file.

The format of this file is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Release and App deployment

Package releases and Companion App deployments are done by the [Unity Live Capture team](CREDITS.md).
