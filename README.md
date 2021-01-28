# DemoLuceneBlazor
Demo of creating a Lucene.NET index and using it inside Blazor WebAssembly (all in the web browser, no server-side components required)!

GitHub repo consists of two projects  
1) .NET 5 Console project that builds a small Lucene index using raw baseball data (~20k)
- The index is packaged up in a compressed zip file for portability and re-use
2) Blazor Web Assembly project that shows how Lucene.NET information retrieval can be done all inside the web browser
- The Lucene.NET index is de-compresssed/streamed into a local browser directory
- Live active searches can be performed using parsing, boolean filters and complete queries
