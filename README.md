#TaskMQ#

**The version of this file is preliminary**

Donations are welcome: 
<a href='http://www.pledgie.com/campaigns/21114'><img alt='Click here to lend your support to: TaskMQ donation and make a donation at www.pledgie.com !' src='http://www.pledgie.com/campaigns/21114.png?skin_name=chrome' border='0' /></a>

##Introduction##

Suppose you have some enterprise application consists with modules implementing BL or other features.
Usually this modules separated by web-services and communicates between each other and with main application, deliver information(e-mail, sms), deliver api level messages, workflow system nodes.

The main idea with this this project is bring control(distribution and testing) on periodic tasks and take it distal from main part of your code, with their specific ifrastructure - like performance ballancing.

Of course for all of that you can use distributed message queues or develop communication network(wcf or some web service stack with REST api or SOAP), **taskmq** tries to do it for you without thinking about deploying and configuration applications, complex distributed architecture instead of this - you can get focus on functional role of your task-specific part(or other meaning of asynchronous/delayed jobs).

The **channel** realise a functional approach, now it only means what in domain part you choose **model** (class), populate and push it to taskmq, *taskmq* picks **channel**, **queue** and eventually invoke specific module.

##Entities##

###Queues###

**Queue** designed for flexible usage and help you with implementing *any flow* you need. Queue consists message *model*, you extend the basic model with any field. Basic model of message contains type string( *MType* ), *Processed* flag, and timestamps. MType used for channel selection, other parameters for *take*(pop) operation which in default selector(fifo selector) - of course you can implement any selector for your fields, for example priority number:
~~~
TQItemSelector selector = new TQItemSelector("Processed", false)
   .Rule("Priority", TQItemSelectorSet.Descending)
   .Rule("AddedTime", TQItemSelectorSet.Ascending);
~~~

And a very important feature of queues: after module process message you can keep message in queue and, or, change field values.

##Configuration, Statistics, Web-access##

Default access module implemented with service stack and self-hosted within application, so you can simply push messages with http protocol at **tmq/q** with PUT method, also at **tmq/c** open access to the configuration. **BBQ** is web interface for simply view and modify all entities on taskmq server, show statictics, maintain queues.

BBQ web interface: <br />![BBQ TaskMQ](doc/tmq-bbq.png "TaskMQ :: BBQ")


Other stuff
-------------
Blueprint project, message queue service with task scheduler<br />
Overview: <br />![Overview](doc/taskmq a.png "Overview")
<br />
Overview: <br />![Entities](doc/TMQ_Entities.png "Entities")
