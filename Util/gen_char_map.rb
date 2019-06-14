

arrayIdentifier = "AsciiToSprite"
first = 'A'.ord;
last = 'Z'.ord;

puts("#{arrayIdentifier}[32] = lookUp.Space;");
puts("#{arrayIdentifier}[35] = lookUp.Hash;");
puts("#{arrayIdentifier}[46] = lookUp.Dot;");
for i in first..last do
	puts("#{arrayIdentifier}[#{i}] = lookUp.#{i.chr};");
end

first = 'a'.ord;
last = 'z'.ord;
for i in first..last do
	puts("#{arrayIdentifier}[#{i}] = lookUp.#{i.chr.upcase}Lower;");
end