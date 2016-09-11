Contributing to PoGo
========================

*TL;DR:*

Thanks for contributing!
-----------
Issues:
- Search through existing issues before you create a new one
- Concise & descriptive issue titles
- Use the template, it's there for a reason. Your post doesn't fit? -> Contact us via social media.

Pull Requests:
- Ask before creating PRs with new features or major rewrites
- Use 4 spaces per indentation level! No tab characters.
- Keep your local master up-to-date, only work in dedicated branches
- Always use `git rebase PoGo/master` from feature branch instead of merging.

Long story:  
We welcome and appreciate contributions from the community.
There are many ways to become involved with PoGo-UWP:
including filing issues,
joining in design conversations,
writing and improving documentation,
and contributing to the code.
Please read the rest of this document to ensure a smooth contribution process.

New to Git?
-----------

* Make sure you have a [GitHub account](https://github.com/signup/free).
* Learning Git:
    * GitHub Help: [Good Resources for Learning Git and GitHub][good-git-resources].
* [GitHub Flow Guide](https://guides.github.com/introduction/flow/):
  step-by-step instructions of GitHub flow.

Contributing to Issues
----------------------

* Check if the issue you are going to file already exists in our [GitHub issues][open-issue].
* If you can't find your issue already,
  [open a new issue](https://github.com/ST-Apps/PoGo-UWP/issues/new),
  making sure to follow the directions as best you can.

Contributing to Code
--------------------
### Forks and Pull Requests

GitHub fosters collaboration through the notion of [pull requests][using-prs].
On GitHub, anyone can [fork][fork-a-repo] an existing repository
into their own user account, where they can make private changes to their fork.
To contribute these changes back into the original repository,
a user simply creates a pull request in order to "request" that the changes be taken "upstream".

Additional references:
* GitHub's guide on [forking](https://guides.github.com/activities/forking/)
* GitHub's guide on [Contributing to Open Source](https://guides.github.com/activities/contributing-to-open-source/#pull-request)
* GitHub's guide on [Understanding the GitHub Flow](https://guides.github.com/introduction/flow/)


### Lifecycle of a pull request

#### Before submitting

* To avoid merge conflicts, make sure your branch [is rebased on the `master`](https://github.com/edx/edx-platform/wiki/How-to-Rebase-a-Pull-Request) branch of this repository.
* Many code changes will require new tests,
  so make sure you've added a new test if existing tests do not effectively test the code changed.
* Clean up your commit history.
  Each commit should be a **single complete** change.
  This discipline is important when reviewing the changes as well as when using `git bisect` and `git revert`.


#### Pull request submission

**Always create a pull request to the `master` branch of this repository**.

* Add a meaningful title of the PR describing what change you want to check in.
  Don't simply put: "Fixes issue #5".
  A better example is: "Add Ensure parameter to New-Item cmdlet", with "Fixes #5" in the PR's body.

* When you create a pull request,
  including a summary of what's included in your changes and
  if the changes are related to an existing GitHub issue,
  please reference the issue in pull request description (e.g. ```Closes #11```).
  See [this][closing-via-message] for more details.

#### Pull Request - Code Review

* After a successful pass,
  the area maintainers will do a code review,
  commenting on any changes that might need to be made.

* Additional feedback is always welcome!
  Even if you are not designated as an area's maintainer,
  feel free to review others' pull requests anyway.
  Leave your comments even if everything looks good;
  a simple "Looks good to me" or "LGTM" will suffice.
  This way we know someone has already taken a look at it!

* Once the code review is done,
  and all merge conflicts are resolved,
  a maintainer should merge your changes.

Common Engineering Practices
----------------------------

We encourage contributors to follow these common engineering practices:

* Format commit messages following these guidelines:

```
Summarize change in 50 characters or less

Similar to email, this is the body of the commit message,
and the above is the subject.
Always leave a single blank line between the subject and the body
so that `git log` and `git rebase` work nicely.

The subject of the commit should use the present tense and
imperative mood, like issuing a command:

> Makes abcd do wxyz

The body should be a useful message explaining
why the changes were made.

If significant alternative solutions were available,
explain why they were discarded.

Keep in mind that the person most likely to refer to your commit message
is you in the future, so be detailed!

As Git commit messages are most frequently viewed in the terminal,
you should wrap all lines around 72 characters.

Using semantic line feeds (breaks that separate ideas)
is also appropriate, as is using Markdown syntax.
```

* These are based on Tim Pope's [guidelines](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html),
  Git SCM [submitting patches](https://git.kernel.org/cgit/git/git.git/tree/Documentation/SubmittingPatches),
  Brandon Rhodes' [semantic linefeeds][],
  and John Gruber's [Markdown syntax](https://daringfireball.net/projects/markdown/syntax).

* Don't commit code that you didn't write.
  If you find code that you think is a good fit to add to PoGo-UWP,
  file an issue and start a discussion before proceeding.

* Create and/or update tests when making code changes.

* Run tests and ensure they are passing before pull request.

* Avoid making big pull requests.
  Before you invest a large amount of time,
  file an issue and start a discussion with the community.

[using-prs]: https://help.github.com/articles/using-pull-requests/
[fork-a-repo]: https://help.github.com/articles/fork-a-repo/
[closing-via-message]: https://help.github.com/articles/closing-issues-via-commit-messages/
[good-git-resources]: https://help.github.com/articles/good-resources-for-learning-git-and-github/
[open-issue]: https://github.com/ST-Apps/PoGo-UWP/issues
[help-wanted-issue]: https://github.com/ST-Apps/PoGo-UWP/issues?q=is%3Aopen+is%3Aissue+label%3A%220%20-%20Backlog%22
[semantic linefeeds]: http://rhodesmill.org/brandon/2012/one-sentence-per-line/
