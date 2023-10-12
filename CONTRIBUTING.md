# Contribution and maintenance

First of all, thank you for considering being a contributor to this repository!

This page includes a set of guidelines for contributors as well as information and instructions about the Live Capture team maintenance process.

## Contribution agreement

**All Contributions to this repository are subject to the [Unity Contribution Agreement (UCA)](https://unity3d.com/legal/licenses/Unity_Contribution_Agreement).**

By making a pull request, you are confirming agreement to the terms and conditions of the UCA, including that your Contributions are your original creation and that you have complete right and authority to make your Contributions.

## How to contribute

### Report bugs

The best way to report bugs is to use the [Unity reporting tool](https://unity.com/releases/editor/qa/bug-reporting).

### Engage with the community

Use the [Unity Forum](https://forum.unity.com/forums/virtual-production.466/) to ask questions, send feedback and discuss with the community.

### Suggest enhancements (via pull requests)

We're not currently accepting feature requests. However, we're open to reviewing [incoming pull requests](#pull-request-workflow), including those that introduce new features. Your contributions will be appreciated.

## Pull request workflow

### Create a branch

Branch from `main` to implement any enhancements or documentation updates, etc.

If you fix a bug, branch off the tagged version with the bug and then make sure to port the fix to `main`.

Use branch naming prefix (e.g. `feature/my-new-feature`)

| Prefix | Meaning |
| :--- | :--- |
| `feature` | A new feature. |
| `ux` | A UX addition or update. |
| `fix` | A bug fix. |
| `docs` | Documentation only changes. |
| `style` | Formatting, missing semi-colons, white-space, etc. |
| `refactor` | A code change that neither fixes a bug nor adds a feature. |
| `perf` | A code change that improves performance. |
| `test` | Adding missing tests. |
| `chore` | Maintain. Changes to the build process or auxiliary tools/libraries/documentation. |

>**Note:**
>* The `main` branch is where the development changes for the next release are. This branch is also known as the "integration branch".
>* We also use publish branches for each major and minor (example: `live-capture/4.0/release`)

### Make a pull request

Once you have a change ready, make a pull request in GitHub.

The pull request description should typically include:
* A summary of the change and its context,
* A description of the fixed issues, and
* Any other relevant information that would help the review.

If you're fixing a bug, also include steps to reproduce, environment information, and screenshots/screencasts as relevant.

### Manage changelog and versioning

All notable changes specific to the Live Capture package should be documented in its specific [Changelog](Packages/com.unity.live-capture/CHANGELOG.md) file.

The format of this file is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Adding changelog entries about fixes and improvements done in the other packages and projects is not required, but will be appreciated.

### Optimize pull request discussions

Discussions on pull requests should be limited to the review of the content (code or documentation) changed via the pull request itself.

## Package release and app deployment

Package releases and companion app deployments are done by the [Unity Live Capture team](CREDITS.md).
