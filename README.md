﻿# HospitalSimulator

  Инфекционное отделение поликлиники имеет смотровую на 𝑁 человек и 𝑀 докторов, которые готовы оказать помощь заболевшим или проконсультировать не заболевших людей. По правилам инфекционного отделения в смотровую может зайти любой человек, если в ней есть свободное место или в нее может зайти не заболевший человек, если в смотровой нет заболевших и наоборот: при наличии свободных мест заболевший человек может войти в смотровую, если там только заболевшие. Как только человек вошел в смотровую он занимает свое место и ожидает доктора. Незанятый доктор выбирает пациента пришедшего раньше других и проводит прием, который длиться от 1 до 𝑇 временных единиц. В особых случаях, доктор может попросить у другого доктора помощи, которая так же может длиться от 1 до 𝑇 временных единиц.  Если все места в смотровой заняты, то пришедшие пациенты встают в очередь. При этом в очереди спустя некоторое время, при наличии нездорового человека, заражается вся очередь. Количество пациентов и интервал их появления произволен и случаен. Напишите программу моделирующую работу инфекционного отделения с использованием средств синхронизации потоков .net framework. Ваша программа должна вести всю историю работы инфекционного отделения: пришедшие пациенты и их состояние, время работы докторов и время оказания помощи пациентам.  Обязательно сохранять информацию о новых заболевших в очереди. Продемонстрируйте работу вашей программы при различных значениях параметров. Подберите параметры так, чтоб показать особые случаи, которые могут возникнуть в инфекционном отделении.
