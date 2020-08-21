# JasonnatorDayTrader Code Repository
Welcome to the free code repository behind my [YouTube channel](https://www.youtube.com/user/Jasonnator/about).

## Getting Started
Be sure to see how the solution is set up and how to get started using it with this video [![Getting Started](https://img.youtube.com/vi/OR5rZHtvlbQ/0.jpg)](https://www.youtube.com/watch?v=OR5rZHtvlbQ&list=PLE_aQ1yU56wbCwJEr343wbFhK0fi2zhLv)

## JDT.CopyFiles
This is a simple console application which copies the appropriate assembly files to the proper locations to support developing indicators and strategies.

## JDT.NT8
This is the main project in the solution.  It contains indicator and strategy code as well as support classes and extensions to help create new tools and algorithms.

## JDT.Test
This is a unit test project ensuring proper functionality of methods and classes.  NinjaTrader itself does not support mocking objects so indicators and strategies are not able to have unit test coverage.  However, the support classes and methods can be designed so they ensure they function as expected.

## Disclaimer
This code is free to use and is covered by the MIT license (more info in LICENSE.md).  **Please, please**, do not blindly use these tools without doing your own due diligence and research before risking live capital.