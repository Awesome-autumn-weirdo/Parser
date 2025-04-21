# Parser

### 1. Вариант задания  
Объявление и определение записи в Pascal
В соответствии с вариантом задания необходимо:  
- Разработать **автоматную грамматику**.  
- Перейти от автоматной грамматики к **конечному автомату** и построить его граф.  
- Выполнить **программную реализацию** алгоритма работы конечного автомата.  
- **Встроить** разработанную программу в интерфейс текстового редактора.

### 2. Примеры допустимых строк
```pascal
type Point = record
    x, y: real
end;

type Person = record
    name: string;
    age: integer
end;
```

### 3. Разработанная грамматика 
1) `<OPR>` → `'type'` `<SPACE>`
2) `<SPACE>` → `' '` `<ID>`
3) `<ID>` → `letter` `<IDREM>`
4) `<IDREM>` → `letter` | `digit` `<IDREM>`
5) `<IDREM>` → `'='` `<REC>`
6) `<REC>` → `'record'` `<SPACE>`
7) `<SPACE>` → `' '` `<FIELD>`
8) `<FIELD>` → `letter` `<FIELDREM>`
9) `<FIELDREM>` → `letter` | `digit` `<FIELDREM>`
10) `<FIELDREM>` → `','` `<FIELDREM>`
11) `<FIELDREM>` → `':'` `<TYPE>`
12) `<TYPE>` → `type` `<END>`
13) `<END>` → `'end'` `<SEMICOLON>`
14) `<SEMICOLON>` → `';'`

`<Digit>` → `0` | `1` | `2` | `3` | `4` | `5` | `6` | `7` | `8` | `9`  
`<Letter>` → `a` | `b` | `c` | ... | `z` | `A` | `B` | `C` | ... | `Z`  

Следуя введенному формальному определению грамматики, представим `G[<OPR>]` ее составляющими:  
- **Z** = `<OPR>`  
- **VT** = `{a, b, c, ..., z, A, B, C, ..., Z, "=", ":", ",", ";", 0, 1, 2, ..., 9, "type", "record", "end", "integer", "real", "char", "boolean", "string" }`  
- **VN** = `{<OPR>, <ID>, <IDREM>, <SPACE>, <FIELD>, <FIELDREM>, <REC>, <TYPE>, <END>, <SEMICOLON>}`  
 

### 4. Классификация грамматики  
Грамматика является автоматной (по Хомскому тип 4): Все правила либо праворекурсивные (A → aB), 
либо терминальные (A → a).

### 5. Граф конечного автомата
![ГрафАвтомата(2) drawio](https://github.com/user-attachments/assets/a6f4f9f9-6fc3-4a98-a945-f5f21a039878)



### 6. Тестовые примеры
![test1](https://github.com/user-attachments/assets/c36986d3-2574-49d1-bb21-bf4b089585af)
![test2](https://github.com/user-attachments/assets/f100ab89-b282-49af-b8f6-c60bb533fed6)
![test3](https://github.com/user-attachments/assets/439e1a32-8485-4296-89b6-b81ae797727d)




