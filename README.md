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
![ГрафАвтомата(1) drawio](https://github.com/user-attachments/assets/a495841d-8783-4557-b374-08c4085b44bf)


### 6. Тестовые примеры
![изображение](https://github.com/user-attachments/assets/399d5492-3f11-44c2-9d99-6fec0eaaf635)
![изображение](https://github.com/user-attachments/assets/8c045c99-5fcd-4bb8-9c78-9e3953ded0ec)


